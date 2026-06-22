using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.UnitTests.Infrastructure;

public class PinValidatorTests
{
    // ──────────────────────────────────────────────
    // PIN correcto → Success
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_DebeRetornarSuccess_CuandoPinCorrecto()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuarioConPin(usuarioId, "hash_del_pin");
        var validator = new PinValidator(
            new FakeUsuarioRepository(usuarioId, usuario),
            new FakePasswordHasher(pinValido: true));

        var resultado = await validator.ValidarAsync(usuarioId, "123456", TestContext.Current.CancellationToken);

        Assert.False(resultado.IsError);
    }

    // ──────────────────────────────────────────────
    // PIN incorrecto → PinIncorrecto
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_DebeRetornarPinIncorrecto_CuandoPinErroneo()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = CrearUsuarioConPin(usuarioId, "hash_del_pin");
        var validator = new PinValidator(
            new FakeUsuarioRepository(usuarioId, usuario),
            new FakePasswordHasher(pinValido: false));

        var resultado = await validator.ValidarAsync(usuarioId, "000000", TestContext.Current.CancellationToken);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PinIncorrecto.Code);
    }

    // ──────────────────────────────────────────────
    // Sin PIN configurado → PinNoConfigurado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_DebeRetornarPinNoConfigurado_CuandoSinPin()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = Usuario.Crear("user", "Nombre", "12345678", "hash_pw").Value;
        // El usuario no tiene PIN establecido (PinHash is null)
        var validator = new PinValidator(
            new FakeUsuarioRepository(usuarioId, usuario),
            new FakePasswordHasher(pinValido: true));

        var resultado = await validator.ValidarAsync(usuarioId, "123456", TestContext.Current.CancellationToken);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PinNoConfigurado.Code);
    }

    // ──────────────────────────────────────────────
    // Usuario no existe → NoEncontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ValidarAsync_DebeRetornarNoEncontrado_CuandoUsuarioInexistente()
    {
        var usuarioId = Guid.NewGuid();
        var validator = new PinValidator(
            new FakeUsuarioRepository(usuarioId, usuario: null),
            new FakePasswordHasher(pinValido: false));

        var resultado = await validator.ValidarAsync(usuarioId, "123456", TestContext.Current.CancellationToken);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NoEncontrado.Code);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Usuario CrearUsuarioConPin(Guid _, string pinHash)
    {
        var usuario = Usuario.Crear("user", "Nombre", "12345678", "hash_pw").Value;
        usuario.EstablecerPin(pinHash);
        return usuario;
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

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
        private readonly bool _pinValido;
        public FakePasswordHasher(bool pinValido) => _pinValido = pinValido;
        public string Hash(string password) => "hash_resultado";
        public bool Verify(string password, string hash) => _pinValido;
    }
}
