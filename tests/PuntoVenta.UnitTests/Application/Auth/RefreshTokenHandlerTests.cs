using Microsoft.Extensions.Logging.Abstractions;
using PuntoVenta.Application.Commands.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Auth;

public class RefreshTokenHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito — rotación normal
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRotarToken_CuandoSesionValida()
    {
        var usuario = CrearUsuario();
        var session = CrearSesionActiva(usuario.Id);
        var handler = CrearHandler(usuario, session);

        var resultado = await handler.Handle(
            new RefreshTokenCommand("token-opaco"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotNull(resultado.Value.AccessToken);
        Assert.NotEmpty(resultado.Value.AccessToken);
        Assert.NotNull(resultado.Value.RefreshToken);
        Assert.NotEmpty(resultado.Value.RefreshToken);
    }

    // ──────────────────────────────────────────────
    // Token no encontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoTokenNoExiste()
    {
        var usuario = CrearUsuario();
        var handler = CrearHandler(usuario, session: null);

        var resultado = await handler.Handle(
            new RefreshTokenCommand("token-inexistente"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.CredencialesInvalidas.Code);
    }

    // ──────────────────────────────────────────────
    // Token expirado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoTokenExpirado()
    {
        var usuario = CrearUsuario();
        var session = CrearSesionExpirada(usuario.Id);
        var handler = CrearHandler(usuario, session);

        var resultado = await handler.Handle(
            new RefreshTokenCommand("token-opaco"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.CredencialesInvalidas.Code);
    }

    // ──────────────────────────────────────────────
    // Usuario inactivo
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoUsuarioInactivo()
    {
        var usuario = CrearUsuario();
        usuario.Desactivar();
        var session = CrearSesionActiva(usuario.Id);
        var handler = CrearHandler(usuario, session);

        var resultado = await handler.Handle(
            new RefreshTokenCommand("token-opaco"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.CredencialesInvalidas.Code);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Usuario CrearUsuario()
        => Usuario.Crear("admin", "Admin Completo", "12345678", "hash").Value;

    private static RefreshTokenSession CrearSesionActiva(Guid usuarioId)
    {
        var ahora = DateTime.UtcNow;
        return RefreshTokenSession.Crear(
            usuarioId,
            "hash-token-opaco",
            ahora,
            ahora.AddDays(7));
    }

    private static RefreshTokenSession CrearSesionExpirada(Guid usuarioId)
    {
        var hace8dias = DateTime.UtcNow.AddDays(-8);
        return RefreshTokenSession.Crear(
            usuarioId,
            "hash-token-opaco",
            hace8dias,
            hace8dias.AddDays(7));
    }

    private static RefreshTokenHandler CrearHandler(Usuario usuario, RefreshTokenSession? session)
        => new RefreshTokenHandler(
            new FakeRefreshRepo(session),
            new FakeUsuarioRepo(usuario),
            new FakeJwt(),
            new FakeOpaque(),
            new FakeSettings(),
            NullLogger<RefreshTokenHandler>.Instance);

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeRefreshRepo : IRefreshTokenSessionRepository
    {
        private readonly RefreshTokenSession? _session;
        public FakeRefreshRepo(RefreshTokenSession? session) => _session = session;

        // El fake calcula el hash igual que FakeOpaque: "hash-" + token
        public Task<RefreshTokenSession?> ObtenerPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(_session?.TokenHash == tokenHash ? _session : null);

        public Task<RefreshTokenSession?> ObtenerReemplazoPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult<RefreshTokenSession?>(null);

        public Task AgregarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ActualizarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RevocarSesionesUsuarioAsync(Guid usuarioId, DateTime revocadoEnUtc, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task EliminarExpiradosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUsuarioRepo : IUsuarioRepository
    {
        private readonly Usuario _usuario;
        public FakeUsuarioRepo(Usuario usuario) => _usuario = usuario;

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _usuario.Id ? _usuario : null);

        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Usuario>, int)>(([], 0));
        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Usuario>>([]);
        public Task<Usuario> AddAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeJwt : IJwtTokenService
    {
        public TokenInfo GenerarToken(Guid usuarioId, string nombreUsuario, bool requiereCambioPassword = false)
            => new("access-token", "jti-fake", DateTime.UtcNow.AddHours(1));

        public TokenInfo GenerarToken(Guid usuarioId, string nombreUsuario, bool requiereCambioPassword, bool esSuperAdmin)
            => GenerarToken(usuarioId, nombreUsuario, requiereCambioPassword);
    }

    private sealed class FakeOpaque : IOpaqueTokenService
    {
        public string GenerarToken() => "nuevo-refresh-token";
        // Coincide con lo que pone FakeRefreshRepo: hash-token-opaco para "token-opaco"
        public string CalcularHash(string token) => "hash-" + token;
    }

    private sealed class FakeSettings : IAuthSettings
    {
        public string Issuer => "test";
        public int RefreshExpiracionDias => 7;
    }
}
