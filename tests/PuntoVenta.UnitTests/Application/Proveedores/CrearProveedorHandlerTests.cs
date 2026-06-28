using PuntoVenta.Application.Commands.Proveedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.UnitTests.Application.Proveedores;

public class CrearProveedorHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearProveedor_CuandoNombreNuevo()
    {
        var repo = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearProveedorCommand("ACME S.A."),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeCrearProveedor_ConTodosLosCampos()
    {
        var repo = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearProveedorCommand("ACME S.A.", "ventas@acme.cr", "2222-3333", "Distribuidor zona norte"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Conflicto — nombre duplicado case-insensitive
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreYaExiste_CaseInsensitive()
    {
        var repo = new FakeProveedorRepository();
        repo.NombresExistentes.Add("ACME S.A.");  // ya normalizado
        var handler = new CrearProveedorHandler(repo);

        // Command con distinta capitalización — debe normalizarse y coincidir
        var resultado = await handler.Handle(
            new CrearProveedorCommand("acme s.a."),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — correo inválido
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoCorreoInvalido()
    {
        var repo = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearProveedorCommand("ACME S.A.", "no-es-correo"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.CorreoInvalido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — nombre vacío
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreVacio()
    {
        var repo = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearProveedorCommand(string.Empty),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProveedorErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeProveedorRepository : IProveedorRepository
    {
        public List<Proveedor> Guardados { get; } = [];
        public HashSet<string> NombresExistentes { get; } = [];

        public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
            => Task.FromResult(NombresExistentes.Contains(nombreNormalizado));

        public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Proveedor>>([]);

        public Task<Proveedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Proveedor?>(null);

        public Task<(IReadOnlyList<Proveedor> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Proveedor>, int)>(([], 0));

        public Task<Proveedor> AddAsync(Proveedor entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Proveedor?>(null);

        public Task<IReadOnlyList<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Proveedor>>([]);

        public Task UpdateAsync(Proveedor entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Proveedor entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
