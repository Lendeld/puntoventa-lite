using PuntoVenta.Application.Commands.Auth;
using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Auth;

public class LoginHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarTokens_CuandoCredencialesCorrectas()
    {
        var usuario = CrearUsuarioActivo("admin", "hash_correcto");
        var handler = CrearHandler(usuario, passwordValido: true);

        var resultado = await handler.Handle(
            new LoginCommand("admin", "password_correcto"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotNull(resultado.Value.AccessToken);
        Assert.NotEmpty(resultado.Value.AccessToken);
        Assert.NotNull(resultado.Value.RefreshToken);
        Assert.NotEmpty(resultado.Value.RefreshToken);
        Assert.False(resultado.Value.RequiresPasswordChange);
    }

    [Fact]
    public async Task Handle_DebeIndicarRequiereCambioPassword_CuandoDebeCambiar()
    {
        var usuario = CrearUsuarioActivo("admin", "hash_correcto");
        usuario.RequerirCambioPassword();
        var handler = CrearHandler(usuario, passwordValido: true);

        var resultado = await handler.Handle(
            new LoginCommand("admin", "p"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.RequiresPasswordChange);
    }

    // ──────────────────────────────────────────────
    // Credenciales inválidas
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoUsuarioNoExiste()
    {
        var handler = CrearHandler(usuario: null, passwordValido: false);

        var resultado = await handler.Handle(
            new LoginCommand("noexiste", "pwd"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.CredencialesInvalidas.Code);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPasswordIncorrecto()
    {
        var usuario = CrearUsuarioActivo("admin", "hash_real");
        var handler = CrearHandler(usuario, passwordValido: false);

        var resultado = await handler.Handle(
            new LoginCommand("admin", "password_malo"),
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
        var usuario = CrearUsuarioActivo("admin", "hash");
        usuario.Desactivar();
        var handler = CrearHandler(usuario, passwordValido: true);

        var resultado = await handler.Handle(
            new LoginCommand("admin", "pwd"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.UsuarioInactivo.Code);
    }

    // ──────────────────────────────────────────────
    // Password temporal expirada
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPasswordTemporalExpirada()
    {
        var usuario = CrearUsuarioActivo("admin", "hash");
        usuario.RequerirCambioPassword(passwordTemporalExpiraEnUtc: DateTime.UtcNow.AddDays(-1));
        var handler = CrearHandler(usuario, passwordValido: true);

        var resultado = await handler.Handle(
            new LoginCommand("admin", "pwd"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordTemporalExpirada.Code);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Usuario CrearUsuarioActivo(string nombreUsuario, string passwordHash)
        => Usuario.Crear(nombreUsuario, "Nombre Completo", "12345678", passwordHash).Value;

    private static LoginHandler CrearHandler(Usuario? usuario, bool passwordValido)
    {
        var repo = new FakeUsuarioRepository(usuario);
        var hasher = new FakePasswordHasher(passwordValido);
        var jwt = new FakeJwtTokenService();
        var opaque = new FakeOpaqueTokenService();
        var refreshRepo = new FakeRefreshTokenRepo();
        var settings = new FakeAuthSettings();
        return new LoginHandler(repo, hasher, jwt, opaque, refreshRepo, settings);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly Usuario? _usuario;
        public FakeUsuarioRepository(Usuario? usuario) => _usuario = usuario;

        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
            => Task.FromResult(_usuario);

        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Usuario>, int)>(([], 0));
        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Usuario>>([]);
        public Task<Usuario> AddAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        private readonly bool _valido;
        public FakePasswordHasher(bool valido) => _valido = valido;
        public string Hash(string password) => "hash";
        public bool Verify(string password, string hash) => _valido;
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public TokenInfo GenerarToken(Guid usuarioId, string nombreUsuario, bool requiereCambioPassword = false)
            => new("fake.jwt.token", "jti-" + Guid.NewGuid(), DateTime.UtcNow.AddHours(1));
    }

    private sealed class FakeOpaqueTokenService : IOpaqueTokenService
    {
        private int _contador = 0;
        public string GenerarToken() => "refresh-token-" + (++_contador);
        public string CalcularHash(string token) => "hash-" + token;
    }

    private sealed class FakeRefreshTokenRepo : IRefreshTokenSessionRepository
    {
        public Task<RefreshTokenSession?> ObtenerPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<RefreshTokenSession?>(null);
        public Task<RefreshTokenSession?> ObtenerReemplazoPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<RefreshTokenSession?>(null);
        public Task AgregarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ActualizarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RevocarSesionesUsuarioAsync(Guid usuarioId, DateTime revocadoEnUtc, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task EliminarExpiradosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuthSettings : IAuthSettings
    {
        public string Issuer => "test";
        public int RefreshExpiracionDias => 7;
    }
}
