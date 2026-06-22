using ErrorOr;
using PuntoVenta.Application.Commands.Usuarios;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.UnitTests.Application.Usuarios;

public class CrearUsuarioHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearUsuario_CuandoDatosValidos()
    {
        var repo = new FakeUsuarioRepository();
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeHashearPassword_YNoGuardarlaEnTextoPlano()
    {
        var repo = new FakeUsuarioRepository();
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        var usuario = Assert.Single(repo.Guardados);
        Assert.Equal("HASH::Temporal1234!", usuario.PasswordHash);
    }

    [Fact]
    public async Task Handle_DebeMarcarDebeCambiarPassword_AlCrearUsuario()
    {
        var repo = new FakeUsuarioRepository();
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        var usuario = Assert.Single(repo.Guardados);
        Assert.True(usuario.DebeCambiarPassword);
    }

    [Fact]
    public async Task Handle_DebeAsignarRol_CuandoRolExiste()
    {
        var repo = new FakeUsuarioRepository();
        var roles = new FakeRolRepository();
        var rol = roles.AgregarRol("Cajero");
        var handler = NuevoHandler(repo, roles);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!", rol.Id),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        var usuario = Assert.Single(repo.Guardados);
        Assert.Equal(rol.Id, usuario.RolId);
    }

    // ──────────────────────────────────────────────
    // Conflictos — no debe persistir
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreUsuarioYaExiste()
    {
        var repo = new FakeUsuarioRepository();
        repo.NombresUsuarioExistentes.Add("jperez");
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoIdentificacionYaExiste()
    {
        var repo = new FakeUsuarioRepository();
        repo.IdentificacionesExistentes.Add("112340567");
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.IdentificacionYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoRolNoExiste()
    {
        var repo = new FakeUsuarioRepository();
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("jperez", "Juan Pérez", "112340567", "Temporal1234!", Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == RolErrors.NoEncontrado.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no debe persistir
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreUsuarioVacio()
    {
        var repo = new FakeUsuarioRepository();
        var handler = NuevoHandler(repo);

        var resultado = await handler.Handle(
            new CrearUsuarioCommand("  ", "Juan Pérez", "112340567", "Temporal1234!"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == UsuarioErrors.NombreUsuarioRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Helpers / fakes
    // ──────────────────────────────────────────────

    private static CrearUsuarioHandler NuevoHandler(
        FakeUsuarioRepository repo,
        FakeRolRepository? roles = null)
        => new(repo, roles ?? new FakeRolRepository(), new FakePasswordHasher());

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"HASH::{password}";
        public bool Verify(string password, string hash) => hash == $"HASH::{password}";
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Guardados { get; } = [];
        public List<string> NombresUsuarioExistentes { get; } = [];
        public List<string> IdentificacionesExistentes { get; } = [];

        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
            => Task.FromResult(NombresUsuarioExistentes.Contains(nombreUsuario));

        public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
            => Task.FromResult(IdentificacionesExistentes.Contains(identificacion));

        public Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Usuario?>(null);

        public Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Usuario>, int)>(([], 0));

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Guardados.FirstOrDefault(u => u.Id == id));

        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(Guardados);

        public Task<Usuario> AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(Usuario entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeRolRepository : IRolRepository
    {
        private readonly List<Rol> _roles = [];

        public Rol AgregarRol(string nombre)
        {
            var rol = Rol.Crear(nombre).Value;
            _roles.Add(rol);
            return rol;
        }

        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExisteNombreExcluyendoAsync(string nombre, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IReadOnlyList<Rol>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Rol>>(_roles);

        public Task<IReadOnlyList<PaginaPermisosRolTabDto>> ObtenerPaginasConPermisosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PaginaPermisosRolTabDto>>([]);

        public Task<(IReadOnlyList<Rol> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Rol>, int)>(([], 0));

        public Task<PermisosRolPorPaginaDto?> ObtenerPermisosAgrupadosPorPaginaAsync(Guid rolId, Guid paginaId, CancellationToken cancellationToken = default)
            => Task.FromResult<PermisosRolPorPaginaDto?>(null);

        public Task ActualizarPermisosAsync(Guid rolId, Guid paginaId, IReadOnlyList<Guid> permisosIds, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<Rol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));

        public Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Rol>>(_roles);

        public Task<Rol> AddAsync(Rol entity, CancellationToken cancellationToken = default)
        {
            _roles.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Rol entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(Rol entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
