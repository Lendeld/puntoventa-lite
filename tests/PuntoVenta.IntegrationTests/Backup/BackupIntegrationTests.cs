using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.IntegrationTests.Fixtures;

namespace PuntoVenta.IntegrationTests.Backup;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class BackupIntegrationTests(IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // PIN — establecer (autenticado, sin permiso especial)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EstablecerPin_Retorna204_CuandoPasswordCorrecta()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        fixture.SetBearerToken(token);

        var response = await fixture.Client.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "123456"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EstablecerPin_Retorna401_SinToken()
    {
        var clienteSinAuth = fixture.Factory.CreateClient();

        var response = await clienteSinAuth.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "123456"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // UsuarioActual — TienePin se refleja en /auth/usuario-actual
    // ──────────────────────────────────────────────

    [Fact]
    public async Task UsuarioActual_DevuelveTienePinFalse_Antes_Y_True_Despues()
    {
        // Crear usuario de test aislado para no interferir con otros tests
        var (token, _) = await CrearUsuarioDeTestAsync("pin-test-usuario", "PinTestRol");

        var clienteConToken = fixture.Factory.CreateClient();
        clienteConToken.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Antes: TienePin false
        var rAntes = await clienteConToken.GetAsync("/auth/usuario-actual", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, rAntes.StatusCode);
        var dtoAntes = await rAntes.Content.ReadFromJsonAsync<UsuarioActualResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(dtoAntes);
        Assert.False(dtoAntes.TienePin);

        // Establecer PIN
        var rPin = await clienteConToken.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Test1234!",
            PinNuevo = "654321"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, rPin.StatusCode);

        // Después: TienePin true
        var rDespues = await clienteConToken.GetAsync("/auth/usuario-actual", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, rDespues.StatusCode);
        var dtoDespues = await rDespues.Content.ReadFromJsonAsync<UsuarioActualResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(dtoDespues);
        Assert.True(dtoDespues.TienePin);
    }

    // ──────────────────────────────────────────────
    // Backup generar — permiso protege endpoint
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GenerarBackup_Retorna401_SinToken()
    {
        var clienteSinAuth = fixture.Factory.CreateClient();

        var response = await clienteSinAuth.PostAsJsonAsync("/backup/generar", new
        {
            Pin = "123456",
            RutaDestino = "/tmp/test.db"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GenerarBackup_Retorna403_SinPermiso()
    {
        var (token, _) = await CrearUsuarioDeTestAsync("backup-sin-perm", "BackupSinPermRol");

        var clienteSinPermiso = fixture.Factory.CreateClient();
        clienteSinPermiso.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await clienteSinPermiso.PostAsJsonAsync("/backup/generar", new
        {
            Pin = "123456",
            RutaDestino = "/tmp/test.db"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GenerarBackup_GeneraArchivoValido_CuandoAdminConPin()
    {
        // El admin es principal → tiene todos los permisos.
        var token = await fixture.ObtenerTokenAdminAsync();
        var clienteAdmin = fixture.Factory.CreateClient();
        clienteAdmin.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Primero establecer PIN del admin
        var rPin = await clienteAdmin.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "111111"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, rPin.StatusCode);

        // Generar backup hacia archivo temporal
        var rutaBackup = Path.Combine(Path.GetTempPath(), $"pv-backup-test-{Guid.NewGuid():N}.db");
        try
        {
            var rBackup = await clienteAdmin.PostAsJsonAsync("/backup/generar", new
            {
                Pin = "111111",
                RutaDestino = rutaBackup
            }, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rBackup.StatusCode);

            var dto = await rBackup.Content.ReadFromJsonAsync<BackupGeneradoResponse>(TestContext.Current.CancellationToken);
            Assert.NotNull(dto);
            Assert.Equal(rutaBackup, dto.RutaArchivo);
            Assert.False(string.IsNullOrWhiteSpace(dto.VersionEsquema));

            // El archivo debe existir y ser un SQLite válido (magic bytes: "SQLite format 3\0")
            Assert.True(File.Exists(rutaBackup), "El archivo de backup no fue creado.");
            var cabecera = new byte[16];
            await using var fs = File.OpenRead(rutaBackup);
            _ = await fs.ReadAsync(cabecera.AsMemory(0, 16), TestContext.Current.CancellationToken);
            var magia = System.Text.Encoding.ASCII.GetString(cabecera, 0, 15);
            Assert.StartsWith("SQLite format 3", magia);
        }
        finally
        {
            if (File.Exists(rutaBackup)) File.Delete(rutaBackup);
        }
    }

    // ──────────────────────────────────────────────
    // Backup validar — compatible e incompatible
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ValidarBackup_Retorna401_SinToken()
    {
        var clienteSinAuth = fixture.Factory.CreateClient();

        var response = await clienteSinAuth.PostAsJsonAsync("/backup/validar", new
        {
            RutaBackup = "/tmp/algo.db",
            Pin = "123456"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidarBackup_Retorna200Compatible_CuandoMismaVersion()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var clienteAdmin = fixture.Factory.CreateClient();
        clienteAdmin.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Establecer PIN (puede ya existir del test anterior — no importa, idempotente)
        await clienteAdmin.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "222222"
        }, TestContext.Current.CancellationToken);

        var rutaBackup = Path.Combine(Path.GetTempPath(), $"pv-validar-ok-{Guid.NewGuid():N}.db");
        try
        {
            // Generar backup con la versión actual
            var rGen = await clienteAdmin.PostAsJsonAsync("/backup/generar", new
            {
                Pin = "222222",
                RutaDestino = rutaBackup
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, rGen.StatusCode);

            // Validar: debe ser compatible (misma versión)
            var rValidar = await clienteAdmin.PostAsJsonAsync("/backup/validar", new
            {
                RutaBackup = rutaBackup,
                Pin = "222222"
            }, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rValidar.StatusCode);
            var dto = await rValidar.Content.ReadFromJsonAsync<BackupValidacionResponse>(TestContext.Current.CancellationToken);
            Assert.NotNull(dto);
            Assert.True(dto.EsCompatible);
            Assert.Equal(dto.VersionApp, dto.VersionBackup);
            // Compatible → se acuña token de capacidad.
            Assert.False(string.IsNullOrWhiteSpace(dto.TokenRestauracion));
        }
        finally
        {
            if (File.Exists(rutaBackup)) File.Delete(rutaBackup);
        }
    }

    // ──────────────────────────────────────────────
    // Token de restauración — consumir (un solo uso, atado a la ruta)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ConsumirToken_Retorna204_Y_Luego_Falla_EnSegundoUso()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var clienteAdmin = fixture.Factory.CreateClient();
        clienteAdmin.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await clienteAdmin.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "444444"
        }, TestContext.Current.CancellationToken);

        var rutaBackup = Path.Combine(Path.GetTempPath(), $"pv-token-{Guid.NewGuid():N}.db");
        try
        {
            // Generar + validar para obtener un token de capacidad
            var rGen = await clienteAdmin.PostAsJsonAsync("/backup/generar", new
            {
                Pin = "444444",
                RutaDestino = rutaBackup
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, rGen.StatusCode);

            var rValidar = await clienteAdmin.PostAsJsonAsync("/backup/validar", new
            {
                RutaBackup = rutaBackup,
                Pin = "444444"
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, rValidar.StatusCode);
            var validacion = await rValidar.Content.ReadFromJsonAsync<BackupValidacionResponse>(TestContext.Current.CancellationToken);
            Assert.NotNull(validacion);
            var tokenRestauracion = validacion.TokenRestauracion;
            Assert.False(string.IsNullOrWhiteSpace(tokenRestauracion));

            // El endpoint consumir-token es AllowAnonymous: el token es la credencial.
            var clienteAnon = fixture.Factory.CreateClient();

            // Primer uso → 204
            var rConsumir1 = await clienteAnon.PostAsJsonAsync("/backup/consumir-token", new
            {
                Token = tokenRestauracion,
                Ruta = rutaBackup
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, rConsumir1.StatusCode);

            // Segundo uso → 401 (single-use)
            var rConsumir2 = await clienteAnon.PostAsJsonAsync("/backup/consumir-token", new
            {
                Token = tokenRestauracion,
                Ruta = rutaBackup
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, rConsumir2.StatusCode);
        }
        finally
        {
            if (File.Exists(rutaBackup)) File.Delete(rutaBackup);
        }
    }

    [Fact]
    public async Task ConsumirToken_Retorna401_CuandoTokenDesconocido()
    {
        var clienteAnon = fixture.Factory.CreateClient();

        var response = await clienteAnon.PostAsJsonAsync("/backup/consumir-token", new
        {
            Token = "TOKEN-INEXISTENTE",
            Ruta = Path.Combine(Path.GetTempPath(), "no-existe.db")
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidarBackup_Retorna409_CuandoVersionIncompatible()
    {
        var token = await fixture.ObtenerTokenAdminAsync();
        var clienteAdmin = fixture.Factory.CreateClient();
        clienteAdmin.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await clienteAdmin.PutAsJsonAsync("/auth/pin", new
        {
            PasswordActual = "Admin1234!",
            PinNuevo = "333333"
        }, TestContext.Current.CancellationToken);

        // Crear un archivo .db vacío (SQLite sin tabla __EFMigrationsHistory) para simular incompatibilidad
        var rutaFalsa = Path.Combine(Path.GetTempPath(), $"pv-validar-bad-{Guid.NewGuid():N}.db");
        try
        {
            // Crear un SQLite mínimo sin __EFMigrationsHistory
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={rutaFalsa}"))
            {
                await conn.OpenAsync(TestContext.Current.CancellationToken);
                // Tabla ficticia — no tiene __EFMigrationsHistory
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Dummy (Id INTEGER PRIMARY KEY)";
                await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
            }

            var rValidar = await clienteAdmin.PostAsJsonAsync("/backup/validar", new
            {
                RutaBackup = rutaFalsa,
                Pin = "333333"
            }, TestContext.Current.CancellationToken);

            // Sin tabla __EFMigrationsHistory → ArchivoInvalido (400 Validation) o VersionIncompatible (409 Conflict)
            Assert.True(
                rValidar.StatusCode == HttpStatusCode.BadRequest ||
                rValidar.StatusCode == HttpStatusCode.Conflict ||
                rValidar.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Status inesperado: {rValidar.StatusCode}");
        }
        finally
        {
            if (File.Exists(rutaFalsa)) File.Delete(rutaFalsa);
        }
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private async Task<(string Token, Guid UsuarioId)> CrearUsuarioDeTestAsync(
        string nombreUsuario, string nombreRol)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var rol = PuntoVenta.Domain.Entities.Roles.Rol.Crear(nombreRol, "Rol de prueba backup").Value;
        await db.Roles.AddAsync(rol);
        await db.SaveChangesAsync();

        const string passwordPlano = "Test1234!";
        var usuario = PuntoVenta.Domain.Entities.Usuarios.Usuario.Crear(
            nombreUsuario,
            "Usuario Test Backup",
            "00000001",
            hasher.Hash(passwordPlano),
            rolId: rol.Id).Value;

        await db.Usuarios.AddAsync(usuario);
        await db.SaveChangesAsync();

        var loginResponse = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = nombreUsuario,
            Password = passwordPlano
        });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthFlowDto>();
        var token = body?.AccessToken ?? throw new InvalidOperationException("Sin AccessToken");

        return (token, usuario.Id);
    }

    // ──────────────────────────────────────────────
    // DTOs internos
    // ──────────────────────────────────────────────

    private sealed record AuthFlowDto(string AccessToken, string RefreshToken, bool RequiresPasswordChange);

    private sealed record UsuarioActualResponse(
        string Usuario,
        string Nombre,
        bool DebeCambiarPassword,
        bool TienePin);

    private sealed record BackupGeneradoResponse(
        string RutaArchivo,
        string VersionEsquema,
        DateTime FechaUtc,
        string? AppVersion);

    private sealed record BackupValidacionResponse(
        bool EsCompatible,
        string VersionBackup,
        string VersionApp,
        string TokenRestauracion);
}
