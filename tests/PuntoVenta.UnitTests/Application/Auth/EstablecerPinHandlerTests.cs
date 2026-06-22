using PuntoVenta.Application.Commands.Auth.EstablecerPinUsuarioActual;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Auth;

public class EstablecerPinHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito — establece PIN y persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeEstablecerPin_CuandoPasswordActualCorrecta()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuario("admin", "hash_password");
        var repo = new FakeUsuarioRepository(usuarioId, usuario);
        var handler = CrearHandler(usuarioId, usuario, passwordValida: true, repo);

        var resultado = await handler.Handle(
            new EstablecerPinUsuarioActualCommand("password_actual", "123456"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(usuario.TienePin);
        Assert.Equal("nuevo_pin_hash", usuario.PinHash);
        Assert.True(repo.UpdateLlamado);
    }

    // ──────────────────────────────────────────────
    // Password incorrecta — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPasswordActualIncorrecta()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuario("admin", "hash_password");
        var repo = new FakeUsuarioRepository(usuarioId, usuario);
        var handler = CrearHandler(usuarioId, usuario, passwordValida: false, repo);

        var resultado = await handler.Handle(
            new EstablecerPinUsuarioActualCommand("password_incorrecta", "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PasswordActualIncorrecta.Code);
        Assert.False(usuario.TienePin);
        Assert.False(repo.UpdateLlamado);
    }

    // ──────────────────────────────────────────────
    // Usuario no encontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoUsuarioNoExiste()
    {
        var usuarioId = Guid.NewGuid();
        var repo = new FakeUsuarioRepository(usuarioId, usuario: null);
        var handler = CrearHandler(usuarioId, usuario: null, passwordValida: false, repo);

        var resultado = await handler.Handle(
            new EstablecerPinUsuarioActualCommand("password", "123456"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NoEncontrado.Code);
        Assert.False(repo.UpdateLlamado);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Usuario CrearUsuario(string nombreUsuario, string passwordHash)
        => Usuario.Crear(nombreUsuario, "Nombre Completo", "12345678", passwordHash).Value;

    private static EstablecerPinUsuarioActualHandler CrearHandler(
        Guid usuarioId, Usuario? usuario, bool passwordValida, FakeUsuarioRepository repo)
    {
        return new EstablecerPinUsuarioActualHandler(
            new FakeUsuarioActual(usuarioId),
            repo,
            new FakePasswordHasher(passwordValida));
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

        public bool UpdateLlamado { get; private set; }

        public FakeUsuarioRepository(Guid usuarioId, Usuario? usuario)
        {
            _usuarioId = usuarioId;
            _usuario = usuario;
        }

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _usuarioId ? _usuario : null);

        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            UpdateLlamado = true;
            return Task.CompletedTask;
        }

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
        private readonly bool _valida;
        public FakePasswordHasher(bool valida) => _valida = valida;
        public string Hash(string password) => "nuevo_pin_hash";
        public bool Verify(string password, string hash) => _valida;
    }
}
