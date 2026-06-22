using PuntoVenta.Application.Commands.Categorias;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.UnitTests.Application.Categorias;

public class CrearCategoriaHandlerTests
{
    // ──────────────────────────────────────────────
    // Éxito
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearCategoria_CuandoNombreNuevo()
    {
        var repo = new FakeCategoriaRepository();
        var handler = new CrearCategoriaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCategoriaCommand("Bebidas"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.NotEqual(Guid.Empty, resultado.Value);
        Assert.Single(repo.Guardados);
    }

    [Fact]
    public async Task Handle_DebeCrearCategoria_ConDescripcion()
    {
        var repo = new FakeCategoriaRepository();
        var handler = new CrearCategoriaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCategoriaCommand("Bebidas", "Bebidas frías y calientes"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Conflicto — nombre duplicado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNombreYaExiste()
    {
        var repo = new FakeCategoriaRepository();
        repo.NombresExistentes.Add("BEBIDAS");
        var handler = new CrearCategoriaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCategoriaCommand("Bebidas"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.NombreYaExiste.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Error de dominio — no persiste
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarErrorDominio_CuandoNombreVacio()
    {
        var repo = new FakeCategoriaRepository();
        var handler = new CrearCategoriaHandler(repo);

        var resultado = await handler.Handle(
            new CrearCategoriaCommand(string.Empty),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == CategoriaErrors.NombreRequerido.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Fakes
    // ──────────────────────────────────────────────

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        public List<Categoria> Guardados { get; } = [];
        public HashSet<string> NombresExistentes { get; } = [];

        public Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
            => Task.FromResult(NombresExistentes.Contains(nombreNormalizado));

        public Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IReadOnlyList<Categoria>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Categoria>>([]);

        public Task<Categoria?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Categoria?>(null);

        public Task<(IReadOnlyList<Categoria> Items, int Total)> ObtenerListaPaginadoAsync(
            int pagina, int tamano, string? filtroDinamico, bool? activo, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Categoria>, int)>(([], 0));

        public Task<Categoria> AddAsync(Categoria entity, CancellationToken cancellationToken = default)
        {
            Guardados.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Categoria?>(null);

        public Task<IReadOnlyList<Categoria>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Categoria>>([]);

        public Task UpdateAsync(Categoria entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Categoria entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
