using PuntoVenta.Application.Commands.Inventarios;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.UnitTests.Application.Inventarios;

public class ObtenerReporteInventarioHandlerTests
{
    // (a) Totales valorizados correctos
    [Fact]
    public async Task Handle_CalculaTotalesCorrectamente()
    {
        // 2 productos: uno con tarifa 13%, otro sin tarifa
        var repo = new FakeReporteInventarioRepository(
        [
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P01", Nombre = "Prod 1",
                Existencia = 10m, PrecioUnitario = 1000m, PrecioCosto = 500m, TarifaPorcentaje = 13m,
                Categoria = "Cat A", FechaCreacion = DateTime.UtcNow,
            },
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P02", Nombre = "Prod 2",
                Existencia = 5m, PrecioUnitario = 2000m, PrecioCosto = 800m, TarifaPorcentaje = 0m,
                Categoria = "Cat B", FechaCreacion = DateTime.UtcNow,
            },
        ]);
        var handler = new ObtenerReporteInventarioHandler(repo);

        var result = await handler.Handle(new ObtenerReporteInventarioQuery(null, null, null), CancellationToken.None);

        Assert.False(result.IsError);
        var dto = result.Value;

        Assert.Equal(2, dto.Filas.Count);
        Assert.Equal(15m, dto.TotalExistencia);                   // 10 + 5

        // P01: precioVenta = 1000 + round(1000*13/100) = 1000 + 130 = 1130; valorVenta = round(10*1130) = 11300
        // P02: precioVenta = 2000 + 0 = 2000; valorVenta = round(5*2000) = 10000
        Assert.Equal(21300m, dto.TotalValorVenta);  // 11300 (P01) + 10000 (P02)

        // P01: valorCosto = round(10*500) = 5000; P02: round(5*800) = 4000
        Assert.Equal(9000m, dto.TotalValorCosto);

        // totalValorImpuesto = round(10*130) + round(5*0) = 1300
        Assert.Equal(1300m, dto.TotalValorImpuesto);
    }

    // (b) PrecioNeto/MontoImpuesto/PrecioVenta derivados de PrecioUnitario (neto) + Tarifa%
    [Fact]
    public async Task Handle_DerivaPreciosCorrectamente()
    {
        var repo = new FakeReporteInventarioRepository(
        [
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P01", Nombre = "Prod 1",
                Existencia = 1m, PrecioUnitario = 1000m, PrecioCosto = null, TarifaPorcentaje = 13m,
                Categoria = "Cat A", FechaCreacion = DateTime.UtcNow,
            },
        ]);
        var handler = new ObtenerReporteInventarioHandler(repo);

        var result = await handler.Handle(new ObtenerReporteInventarioQuery(null, null, null), CancellationToken.None);

        Assert.False(result.IsError);
        var fila = result.Value.Filas[0];

        Assert.Equal(1000m, fila.PrecioNeto);
        Assert.Equal(130m, fila.MontoImpuesto);
        Assert.Equal(1130m, fila.PrecioVenta);
        Assert.Equal(0m, fila.PrecioCosto);   // PrecioCosto null -> 0
    }

    // (c) CategoriaId null -> categoria = ""
    [Fact]
    public async Task Handle_CategoriaNula_DevuelveCadenaVacia()
    {
        var repo = new FakeReporteInventarioRepository(
        [
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P01", Nombre = "Prod 1",
                Existencia = 1m, PrecioUnitario = 100m, PrecioCosto = null, TarifaPorcentaje = 0m,
                Categoria = null,   // sin categoría
                FechaCreacion = DateTime.UtcNow,
            },
        ]);
        var handler = new ObtenerReporteInventarioHandler(repo);

        var result = await handler.Handle(new ObtenerReporteInventarioQuery(null, null, null), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(string.Empty, result.Value.Filas[0].Categoria);
    }

    // (d) Cap de filas -> error de validación
    [Fact]
    public async Task Handle_ExcedeCapFilas_RetornaError()
    {
        // El fake devuelve MaxFilasReporteInventario + 1 filas (simula que el repo leyó tope+1)
        var filas = Enumerable.Range(0, IProductoRepository.MaxFilasReporteInventario + 1)
            .Select(i => new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = $"P{i:D6}", Nombre = $"Prod {i}",
                Existencia = 1m, PrecioUnitario = 100m, TarifaPorcentaje = 0m,
                FechaCreacion = DateTime.UtcNow,
            })
            .ToList();

        var repo = new FakeReporteInventarioRepository(filas);
        var handler = new ObtenerReporteInventarioHandler(repo);

        var result = await handler.Handle(new ObtenerReporteInventarioQuery(null, null, null), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "ReporteInventario_DemasiadasFilas");
    }

    // (e) ProveedorId null -> proveedor = "" ; nombre de proveedor se copia tal cual
    [Fact]
    public async Task Handle_ProveedorNulo_DevuelveCadenaVacia_YNombreSeCopia()
    {
        var repo = new FakeReporteInventarioRepository(
        [
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P01", Nombre = "Prod 1",
                Existencia = 1m, PrecioUnitario = 100m, TarifaPorcentaje = 0m,
                Proveedor = null,   // sin proveedor
                FechaCreacion = DateTime.UtcNow,
            },
            new InventarioReporteProyeccionDto
            {
                ProductoId = Guid.NewGuid(), Codigo = "P02", Nombre = "Prod 2",
                Existencia = 1m, PrecioUnitario = 100m, TarifaPorcentaje = 0m,
                Proveedor = "Prov X",
                FechaCreacion = DateTime.UtcNow,
            },
        ]);
        var handler = new ObtenerReporteInventarioHandler(repo);

        var result = await handler.Handle(new ObtenerReporteInventarioQuery(null, null, null), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(string.Empty, result.Value.Filas[0].Proveedor);
        Assert.Equal("Prov X", result.Value.Filas[1].Proveedor);
    }

    // (f) ProveedorId se pasa al repo (el filtrado real lo hace EF)
    [Fact]
    public async Task Handle_PasaProveedorIdAlRepo()
    {
        var repo = new FakeReporteInventarioRepository([]);
        var handler = new ObtenerReporteInventarioHandler(repo);
        var unGuid = Guid.NewGuid();

        await handler.Handle(new ObtenerReporteInventarioQuery(null, null, unGuid), CancellationToken.None);

        Assert.Equal(unGuid, repo.CapturedProveedorId);
    }

    // ── Fake privado ────────────────────────────────────────────────────────────

    private sealed class FakeReporteInventarioRepository(
        IReadOnlyList<InventarioReporteProyeccionDto> proyeccion) : IProductoRepository
    {
        public Guid? CapturedProveedorId { get; private set; }

        public Task<IReadOnlyList<InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(
            string? codigo, Guid? categoriaId, Guid? proveedorId, int maxFilas, CancellationToken cancellationToken = default)
        {
            CapturedProveedorId = proveedorId;
            return Task.FromResult(proyeccion);
        }

        // Resto de la interfaz — no usados en estos tests
        public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoExcluyendoAsync(string codigo, Guid excludeId, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoBarrasExcluyendoAsync(string codigoBarras, Guid excludeId, CancellationToken ct = default) => Task.FromResult(false);
        public Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(int pagina, int tamano, string? filtroDinamico, TipoItem? tipoItem, Guid? categoriaId, CancellationToken ct = default)
            => Task.FromResult<(IReadOnlyList<Producto>, int)>(([], 0));
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<Producto> AddAsync(Producto entity, CancellationToken ct = default) => Task.FromResult(entity);
        public Task UpdateAsync(Producto entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Producto entity, CancellationToken ct = default) => Task.CompletedTask;
    }
}
