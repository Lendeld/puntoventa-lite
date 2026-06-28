using PuntoVenta.Application.Commands.Inventario;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Inventarios;

public class AjustarStockProductoHandlerTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Producto CrearProductoConStock(decimal existencia)
    {
        var p = Producto.Crear("AJ01", "Ajuste Test", TipoItem.Bien, 500m,
            tarifaIvaImpuestoCodigo: "08").Value;
        if (existencia > 0)
            p.AplicarMovimientoStock(existencia);
        return p;
    }

    private static AjustarStockProductoHandler CrearHandler(Producto producto, FakeMovimientoStockRepository? movRepo = null)
    {
        var repo = new FakeAjusteProductoRepository(producto);
        return new AjustarStockProductoHandler(
            repo,
            movRepo ?? new FakeMovimientoStockRepository(),
            new FakeUsuarioActualVentas(),
            new FakeFechaActual());
    }

    // ──────────────────────────────────────────────
    // Salida sin stock suficiente → rechaza
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRechazar_CuandoDeltaNegativoExcedeExistencia()
    {
        var producto = CrearProductoConStock(5m);
        var movRepo = new FakeMovimientoStockRepository();
        var handler = CrearHandler(producto, movRepo);

        var resultado = await handler.Handle(
            new AjustarStockProductoCommand(producto.Id, Delta: -10m),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Empty(movRepo.Guardados);  // sin movimiento
        Assert.Equal(5m, producto.Existencia); // sin cambio
    }

    // ──────────────────────────────────────────────
    // Salida igual a existencia → permite (queda en 0)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebePermitir_CuandoDeltaNegativoIgualAExistencia()
    {
        var producto = CrearProductoConStock(5m);
        var movRepo = new FakeMovimientoStockRepository();
        var handler = CrearHandler(producto, movRepo);

        var resultado = await handler.Handle(
            new AjustarStockProductoCommand(producto.Id, Delta: -5m),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(movRepo.Guardados);
        Assert.Equal(0m, producto.Existencia);
    }

    // ──────────────────────────────────────────────
    // Ingreso (Delta > 0) no valida stock
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebePermitir_CuandoDeltaPositivo()
    {
        var producto = CrearProductoConStock(0m);
        var movRepo = new FakeMovimientoStockRepository();
        var handler = CrearHandler(producto, movRepo);

        var resultado = await handler.Handle(
            new AjustarStockProductoCommand(producto.Id, Delta: 20m),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(movRepo.Guardados);
        Assert.Equal(20m, producto.Existencia);
    }

    // ── Fake privado ────────────────────────────────────────────────────────

    private sealed class FakeAjusteProductoRepository(Producto producto) : IProductoRepository
    {
        public Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult<Producto?>(producto);

        // Resto — no usados
        public Task<bool> ExisteCodigoAsync(string c, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoExcluyendoAsync(string c, Guid id, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoBarrasAsync(string c, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExisteCodigoBarrasExcluyendoAsync(string c, Guid id, CancellationToken ct = default) => Task.FromResult(false);
        public Task<Producto?> ObtenerPorCodigoBarrasAsync(string c, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(int p, int t, string? f, TipoItem? ti, Guid? cat, CancellationToken ct = default) => Task.FromResult<(IReadOnlyList<Producto>, int)>(([], 0));
        public Task<IReadOnlyList<InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(string? c, Guid? cat, int max, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<InventarioReporteProyeccionDto>>([]);
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);
        public Task<Producto> AddAsync(Producto e, CancellationToken ct = default) => Task.FromResult(e);
        public Task UpdateAsync(Producto e, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Producto e, CancellationToken ct = default) => Task.CompletedTask;
    }
}
