using PuntoVenta.Application.Commands.Vendedores;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.UnitTests.Application.Vendedores;

public class CrearVendedorHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearVendedor_CuandoNombreNuevo()
    {
        var repo = new FakeVendedorRepository();
        var handler = new CrearVendedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearVendedorCommand("María García"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeAsignarPrincipal_CuandoEsPrimerVendedor()
    {
        var repo = new FakeVendedorRepository();
        var handler = new CrearVendedorHandler(repo);

        await handler.Handle(new CrearVendedorCommand("Primer Vendedor"), CancellationToken.None);

        Assert.True(repo.Guardados[0].IsPrincipal);
    }

    [Fact]
    public async Task Handle_DebeMantenerPrincipalExplicitamente_CuandoSePideComoPrincipal()
    {
        var repo = new FakeVendedorRepository();
        var handler = new CrearVendedorHandler(repo);

        // Crear primero un vendedor (queda como principal)
        await handler.Handle(new CrearVendedorCommand("Vendedor A"), CancellationToken.None);

        // Crear segundo vendedor como principal — el primero debe perder el flag
        await handler.Handle(new CrearVendedorCommand("Vendedor B", IsPrincipal: true), CancellationToken.None);

        Assert.False(repo.Guardados[0].IsPrincipal);
        Assert.True(repo.Guardados[1].IsPrincipal);
    }

    // ──────────────────────────────────────────────
    // Conflicto — nombre duplicado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreYaExiste()
    {
        var repo = new FakeVendedorRepository();
        repo.NombresExistentes.Add("MARÍA GARCÍA");
        var handler = new CrearVendedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearVendedorCommand("María García"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == VendedorErrors.NombreYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoNombreVacio()
    {
        var repo = new FakeVendedorRepository();
        var handler = new CrearVendedorHandler(repo);

        var resultado = await handler.Handle(
            new CrearVendedorCommand(string.Empty),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == VendedorErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeVendedorRepository : IVendedorRepository
    {
        public List<Vendedor> Guardados { get; } = [];
        public HashSet<string> NombresExistentes { get; } = [];

        public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
            => Task.FromResult(NombresExistentes.Contains(nombreNormalizado));

        public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IReadOnlyList<Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Vendedor>>([]);

        public Task<Vendedor?> ObtenerPrincipalAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Guardados.FirstOrDefault(v => v.IsPrincipal));

        public Task<Vendedor?> ObtenerPrincipalEditableAsync(Guid? excludeId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Guardados.FirstOrDefault(v => v.IsPrincipal && (excludeId is null || v.Id != excludeId)));

        public Task<Vendedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Vendedor?>(null);

        public Task<(IReadOnlyList<Vendedor> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Vendedor>, int)>(([], 0));

        public Task<Vendedor> AddAsync(Vendedor entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Vendedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Vendedor?>(null);

        public Task<IReadOnlyList<Vendedor>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Vendedor>>([]);

        public Task UpdateAsync(Vendedor entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Vendedor entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
