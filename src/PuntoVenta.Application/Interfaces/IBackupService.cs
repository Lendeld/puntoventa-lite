using ErrorOr;
using PuntoVenta.Application.DTOs.Backup;

namespace PuntoVenta.Application.Interfaces;

public interface IBackupService
{
    /// <summary>
    /// Genera un backup consistente del SQLite activo usando VACUUM INTO hacia la ruta destino indicada.
    /// El archivo destino es creado directamente por el comando — no se usa un temporal intermedio.
    /// </summary>
    Task<ErrorOr<BackupGeneradoDto>> GenerarAsync(string rutaDestino, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida que el archivo en la ruta indicada sea un SQLite válido con la misma versión de
    /// migración que la base de datos activa. Abre el archivo en modo ReadOnly con una conexión
    /// independiente para no interferir con la DB viva.
    /// </summary>
    Task<ErrorOr<BackupValidacionDto>> ValidarAsync(string rutaBackup, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el identificador de la última migración aplicada en la base de datos activa.
    /// Corresponde al último registro de __EFMigrationsHistory ordenado por MigrationId DESC.
    /// </summary>
    Task<string> ObtenerVersionEsquemaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula la huella SHA-256 (hex) del archivo indicado. Devuelve string vacío si el
    /// archivo no existe o no se puede leer. Se usa para atar el token de restauración al
    /// contenido exacto validado y detectar si el archivo cambió antes del swap.
    /// </summary>
    Task<string> CalcularHuellaAsync(string ruta, CancellationToken cancellationToken = default);
}
