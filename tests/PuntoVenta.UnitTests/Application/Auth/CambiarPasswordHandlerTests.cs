using PuntoVenta.Application.Commands.Auth.CambiarPasswordUsuarioActual;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Auth;

public class CambiarPasswordHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarNuevoToken_CuandoPasswordActualCorrecta()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuario("admin", "hash_actual");
        var handler = CrearHandler(usuarioId, usuario, passwordActualValido: true);

        var resultado = await handler.Handle(
            new CambiarPasswordUsuarioActualCommand("actual", "nueva"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotNull(resultado.Value.AccessToken);
        Assert.NotEmpty(resultado.Value.AccessToken);
        Assert.False(resultado.Value.RequiresPasswordChange);
    }

    [Fact]
    public async Task Handle_DebeMarcarCambioCompletado_CuandoExitosoConDebeCAmbiar()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuario("admin", "hash_actual");
        usuario.RequerirCambioPassword();
        Assert.True(usuario.DebeCambiarPassword);

        var handler = CrearHandler(usuarioId, usuario, passwordActualValido: true);

        var resultado = await handler.Handle(
            new CambiarPasswordUsuarioActualCommand("actual", "nueva"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.False(usuario.DebeCambiarPassword);
    }

    // ──────────────────────────────────────────────
    // Password actual incorrecta
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPasswordActualIncorrecta()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuario("admin", "hash_actual");
        var handler = CrearHandler(usuarioId, usuario, passwordActualValido: false);

        var resultado = await handler.Handle(
            new CambiarPasswordUsuarioActualCommand("incorrecta", "nueva"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordActualIncorrecta.Code);
    }

    // ──────────────────────────────────────────────
    // Usuario no encontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoUsuarioNoExiste()
    {
        var usuarioId = Guid.NewGuid();
        var handler = CrearHandler(usuarioId, usuario: null, passwordActualValido: false);

        var resultado = await handler.Handle(
            new CambiarPasswordUsuarioActualCommand("actual", "nueva"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NoEncontrado.Code);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Usuario CrearUsuario(string nombreUsuario, string passwordHash)
        => Usuario.Crear(nombreUsuario, "Nombre Completo", "12345678", passwordHash).Value;

    private static CambiarPasswordUsuarioActualHandler CrearHandler(
        Guid usuarioId, Usuario? usuario, bool passwordActualValido)
    {
        return new CambiarPasswordUsuarioActualHandler(
            new FakeUsuarioActual(usuarioId),
            new FakeUsuarioRepository(usuarioId, usuario),
            new FakePasswordHasher(passwordActualValido),
            new FakeRefreshTokenRepo(),
            new FakeAuthSettings(),
            new FakeJwtTokenService(),
            new FakeOpaqueTokenService());
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeUsuarioActual : IUsuarioActual
    {
        public FakeUsuarioActual(Guid id) => UsuarioId = id;
        public Guid UsuarioId { get; }
        public string NombreUsuario => "admin";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly Guid _usuarioId;
        private readonly Usuario? _usuario;

        public FakeUsuarioRepository(Guid usuarioId, Usuario? usuario)
        {
            _usuarioId = usuarioId;
            _usuario = usuario;
        }

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _usuarioId ? _usuario : null);

        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Usuario>, int)>(([], 0));
        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Usuario>>([]);
        public Task<Usuario> AddAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task DeleteAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        private readonly bool _valido;
        public FakePasswordHasher(bool valido) => _valido = valido;
        public string Hash(string password) => "nuevo_hash";
        public bool Verify(string password, string hash) => _valido;
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

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public TokenInfo GenerarToken(Guid usuarioId, string nombreUsuario, bool requiereCambioPassword = false)
            => new("fake.jwt.token", "jti", DateTime.UtcNow.AddHours(1));
    }

    private sealed class FakeOpaqueTokenService : IOpaqueTokenService
    {
        public string GenerarToken() => "refresh-token";
        public string CalcularHash(string token) => "hash-" + token;
    }

    private sealed class FakeAuthSettings : IAuthSettings
    {
        public string Issuer => "test";
        public int RefreshExpiracionDias => 7;
    }
}
