using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Usuarios;

public class ActualizarUsuarioHandlerTests
{
    [Fact]
    public async Task Handle_DebeBloquear_CuandoOtroEditaAlPropietario()
    {
        var repo = new FakeUsuarioRepository();
        var propietario = CrearPropietario(Guid.NewGuid());
        repo.Seed(propietario);
        var handler = NuevoHandler(repo, usuarioActualId: Guid.NewGuid()); // otro usuario

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(propietario.Id, Activo: false),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PropietarioSoloSeEditaASiMismo.Code);
        Assert.False(repo.Actualizado);
    }

    [Fact]
    public async Task Handle_DebePermitir_CuandoElPropietarioSeEditaASiMismo()
    {
        var repo = new FakeUsuarioRepository();
        var propietario = CrearPropietario(Guid.NewGuid());
        repo.Seed(propietario);
        var handler = NuevoHandler(repo, usuarioActualId: propietario.Id);

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(propietario.Id, Activo: true),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(repo.Actualizado);
        Assert.True(propietario.Activo);
    }

    [Fact]
    public async Task Handle_DebeBloquearCambioDeRol_AunSiEsElMismoPropietario()
    {
        var repo = new FakeUsuarioRepository();
        var propietario = CrearPropietario(Guid.NewGuid());
        repo.Seed(propietario);
        var handler = NuevoHandler(repo, usuarioActualId: propietario.Id);

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(propietario.Id, Activo: true, RolId: Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PropietarioRolNoSePuedeCambiar.Code);
        Assert.False(repo.Actualizado);
    }

    [Fact]
    public async Task Handle_DebeBloquear_CuandoElPropietarioSeAutodesactiva()
    {
        var repo = new FakeUsuarioRepository();
        var propietario = CrearPropietario(Guid.NewGuid());
        repo.Seed(propietario);
        var handler = NuevoHandler(repo, usuarioActualId: propietario.Id);

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(propietario.Id, Activo: false),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.PropietarioNoSePuedeDesactivar.Code);
        Assert.False(repo.Actualizado);
        Assert.True(propietario.Activo);
    }

    [Fact]
    public async Task Handle_DebePermitir_CuandoOtroEditaUsuarioNoPropietario()
    {
        var repo = new FakeUsuarioRepository();
        var usuario = Usuario.Crear("jperez", "Juan", "112340567", "HASH").Value;
        repo.Seed(usuario);
        var handler = NuevoHandler(repo, usuarioActualId: Guid.NewGuid());

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(usuario.Id, Activo: false),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(repo.Actualizado);
    }

    // ── Helpers / fakes ──────────────────────────────────────────────────────

    private static Usuario CrearPropietario(Guid rolId)
    {
        var usuario = Usuario.Crear("admin", "Administrador", "0000000000", "HASH", rolId: rolId).Value;
        usuario.MarcarComoPropietario();
        return usuario;
    }

    private static ActualizarUsuarioHandler NuevoHandler(FakeUsuarioRepository repo, Guid usuarioActualId)
        => new(
            new FakeUsuarioActual(usuarioActualId),
            repo,
            new FakeRolRepository(),
            new FakePermisoCache());

    private sealed class FakeUsuarioActual(Guid usuarioId) : IUsuarioActual
    {
        public Guid UsuarioId { get; } = usuarioId;
        public string NombreUsuario => "actual";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FakePermisoCache : IPermisoCache
    {
        public Task<IReadOnlyList<string>> ObtenerPermisosAsync(Guid usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>([]);
        public void Invalidar(Guid usuarioId) { }
        public void InvalidarTodos() { }
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = [];
        public bool Actualizado { get; private set; }

        public void Seed(Usuario usuario) => _usuarios.Add(usuario);

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            Actualizado = true;
            return Task.CompletedTask;
        }

        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Usuario>, int)>(([], 0));
        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Usuario>>(_usuarios);
        public Task<Usuario> AddAsync(Usuario entity, CancellationToken cancellationToken = default) { _usuarios.Add(entity); return Task.FromResult(entity); }
        public Task DeleteAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeRolRepository : IRolRepository
    {
        public Task<Rol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Rol?>(null);
        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExisteNombreExcluyendoAsync(string nombre, Guid excludeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlyList<Rol>> ObtenerActivosAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Rol>>([]);
        public Task<IReadOnlyList<PaginaPermisosRolTabDto>> ObtenerPaginasConPermisosAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PaginaPermisosRolTabDto>>([]);
        public Task<(IReadOnlyList<Rol> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default) => Task.FromResult<(IReadOnlyList<Rol>, int)>(([], 0));
        public Task<PermisosRolPorPaginaDto?> ObtenerPermisosAgrupadosPorPaginaAsync(Guid rolId, Guid paginaId, CancellationToken cancellationToken = default) => Task.FromResult<PermisosRolPorPaginaDto?>(null);
        public Task ActualizarPermisosAsync(Guid rolId, Guid paginaId, IReadOnlyList<Guid> permisosIds, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Rol>>([]);
        public Task<Rol> AddAsync(Rol entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task UpdateAsync(Rol entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Rol entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
