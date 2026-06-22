using System.Security.Cryptography;
using ErrorOr;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Common.Errors;
using PuntoVenta.Application.DTOs.Backup;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.Infrastructure.Services;

/// <summary>
/// Genera y valida backups del SQLite local usando VACUUM INTO.
///
/// VACUUM INTO seguridad de ruta: SQLite no soporta parámetros bind en VACUUM INTO,
/// por lo que la ruta se incrusta en el texto SQL. Se sanitiza duplicando las comillas
/// simples presentes en la ruta (escape estándar SQL) para evitar inyección.
/// Nunca se acepta entrada no validada por el handler anterior (autenticado + permiso).
/// </summary>
public sealed class SqliteBackupService(
    ApplicationDbContext context,
    IFechaActual fechaActual,
    IInfoSistema infoSistema) : IBackupService, IScopedService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IInfoSistema _infoSistema = infoSistema;

    public async Task<ErrorOr<BackupGeneradoDto>> GenerarAsync(
        string rutaDestino,
        CancellationToken cancellationToken = default)
    {
        // VACUUM INTO falla si el archivo destino ya existe, pero el diálogo de
        // "Guardar como" permite seleccionar un .db existente con confirmación de
        // sobrescritura. Por eso generamos a un archivo temporal en el mismo
        // directorio y luego lo movemos atómicamente sobre el destino.
        var directorio = Path.GetDirectoryName(rutaDestino);
        if (string.IsNullOrEmpty(directorio))
        {
            return BackupErrors.RutaInvalida;
        }

        var rutaTemporal = Path.Combine(
            directorio,
            $".{Path.GetFileName(rutaDestino)}.tmp-{Guid.NewGuid():N}");

        // Sanitizar la ruta escapando comillas simples (estándar SQL: ' → '')
        var rutaEscapada = rutaTemporal.Replace("'", "''");

        var conexion = _context.Database.GetDbConnection();
        await conexion.OpenAsync(cancellationToken);

        try
        {
            using var cmd = conexion.CreateCommand();
            // SQLite no admite parámetros bind en VACUUM INTO, así que la ruta se incrusta en el
            // texto SQL. Está validada (absoluta + canonicalizada por el handler) y con comillas
            // simples escapadas (' → ''). Falso positivo de SQLi: no es parametrizable aquí.
            cmd.CommandText = $"VACUUM INTO '{rutaEscapada}'"; // nosemgrep: csharp.lang.security.sqli.csharp-sqli.csharp-sqli
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            // Reemplazo atómico del destino (sobrescribe si ya existía).
            File.Move(rutaTemporal, rutaDestino, overwrite: true);
        }
        catch (Exception ex) when (ex is SqliteException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            TryEliminar(rutaTemporal);
            return BackupErrors.FalloAlGenerar;
        }

        var versionEsquema = await ObtenerVersionEsquemaAsync(cancellationToken);

        return new BackupGeneradoDto
        {
            RutaArchivo = rutaDestino,
            VersionEsquema = versionEsquema,
            FechaUtc = _fechaActual.AhoraUtc,
            AppVersion = _infoSistema.BackendVersion,
        };
    }

    public async Task<ErrorOr<BackupValidacionDto>> ValidarAsync(
        string rutaBackup,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(rutaBackup))
        {
            return BackupErrors.ArchivoInvalido;
        }

        string versionBackup;

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = rutaBackup,
                Mode = SqliteOpenMode.ReadOnly,
            }.ToString();

            await using var conexionBackup = new SqliteConnection(connectionString);
            await conexionBackup.OpenAsync(cancellationToken);

            using var cmd = conexionBackup.CreateCommand();
            cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1";

            var resultado = await cmd.ExecuteScalarAsync(cancellationToken);
            if (resultado is null or DBNull)
            {
                return BackupErrors.ArchivoInvalido;
            }

            versionBackup = (string)resultado;
        }
        catch (SqliteException)
        {
            return BackupErrors.ArchivoInvalido;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            return BackupErrors.ArchivoInvalido;
        }

        // Compara contra la última migración del ensamblado (no contra la aplicada en la DB viva),
        // según Decisiones del feature: garantiza que el backup coincide exactamente con lo que
        // el binario actual espera, incluso si el binario trae una migración aún no aplicada.
        var migracionesEnsamblado = _context.Database.GetMigrations().ToList();
        var versionApp = migracionesEnsamblado.Count > 0
            ? migracionesEnsamblado[^1]
            : string.Empty;

        var esCompatible = string.Equals(versionBackup, versionApp, StringComparison.Ordinal);

        if (!esCompatible)
        {
            return BackupErrors.VersionIncompatible;
        }

        return new BackupValidacionDto
        {
            EsCompatible = true,
            VersionBackup = versionBackup,
            VersionApp = versionApp,
        };
    }

    /// <summary>Borra un archivo si existe, ignorando errores (limpieza best-effort).</summary>
    private static void TryEliminar(string ruta)
    {
        try
        {
            if (File.Exists(ruta)) File.Delete(ruta);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // best-effort: el temporal quedará para limpieza del SO
        }
    }

    public async Task<string> ObtenerVersionEsquemaAsync(CancellationToken cancellationToken = default)
    {
        var conexion = _context.Database.GetDbConnection();
        await conexion.OpenAsync(cancellationToken);

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1";

        var resultado = await cmd.ExecuteScalarAsync(cancellationToken);
        return resultado is string migrationId ? migrationId : string.Empty;
    }

    public async Task<string> CalcularHuellaAsync(string ruta, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var fs = File.OpenRead(ruta);
            using var sha = SHA256.Create();
            var hash = await sha.ComputeHashAsync(fs, cancellationToken);
            return Convert.ToHexString(hash);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }
}
