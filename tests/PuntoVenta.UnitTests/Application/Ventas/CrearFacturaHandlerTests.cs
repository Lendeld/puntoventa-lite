using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class CrearFacturaHandlerTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Producto CrearProductoConStock(decimal existencia, bool noAplicaExistencias = false)
    {
        var p = Producto.Crear("P-TEST", "Producto Test", TipoItem.Bien, 1000m,
            noAplicaExistencias: noAplicaExistencias).Value;
        if (existencia > 0)
            p.AplicarMovimientoStock(existencia);
        return p;
    }

    private static CrearFacturaCommand ComandoFactura(
        Guid productoId,
        decimal cantidad,
        decimal precio = 1000m,
        Guid? productoId2 = null,
        decimal cantidad2 = 0m)
    {
        var lineas = new List<DocumentoVentaLineaCommand>
        {
            new(productoId, cantidad, precio, 0m)
        };

        if (productoId2.HasValue && cantidad2 > 0)
            lineas.Add(new DocumentoVentaLineaCommand(productoId2.Value, cantidad2, precio, 0m));

        return new CrearFacturaCommand(
            ClienteId: null,
            VendedorId: null,
            CondicionVentaCodigo: "01",
            Lineas: lineas,
            Pagos:
            [
                new DocumentoVentaPagoCommand(
                    "CRC", 1m, "01",
                    cantidad * precio + (cantidad2 > 0 ? cantidad2 * precio : 0m),
                    cantidad * precio + (cantidad2 > 0 ? cantidad2 * precio : 0m),
                    cantidad * precio + (cantidad2 > 0 ? cantidad2 * precio : 0m),
                    0m, 0m)
            ]);
    }

    private static CrearFacturaHandler CrearHandler(
        FakeProductoRepository productoRepo,
        FakeDocumentoVentaRepository? docRepo = null,
        FakeMovimientoStockRepository? movRepo = null,
        FakeUnitOfWork? unitOfWork = null)
    {
        return new CrearFacturaHandler(
            new FakeFechaActual(),
            new FakeUsuarioActualVentas(),
            unitOfWork ?? new FakeUnitOfWork(),
            docRepo ?? new FakeDocumentoVentaRepository(),
            new FakeCondicionVentaRepository(DominioHelper.CrearCondicionVenta()),
            new FakeClienteRepositoryVentas(),
            new FakeVendedorRepositoryVentas(),
            new FakeMedioPagoRepository(DominioHelper.CrearMedioPago()),
            productoRepo,
            new FakeTarifaRepository(),
            new FakeSecuenciaRepository(),
            movRepo ?? new FakeMovimientoStockRepository(),
            new FakeDocumentoVentaEventoService());
    }

    // ──────────────────────────────────────────────
    // Faltante bloquea factura
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRechazar_CuandoCantidadExcedeExistencia()
    {
        var producto = CrearProductoConStock(existencia: 5m);
        var docRepo = new FakeDocumentoVentaRepository();
        var movRepo = new FakeMovimientoStockRepository();
        var productoRepo = new FakeProductoRepository(producto);

        var handler = CrearHandler(productoRepo, docRepo, movRepo);
        var comando = ComandoFactura(producto.Id, cantidad: 10m);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Empty(docRepo.Actualizados); // factura no se persiste
        Assert.Empty(movRepo.Guardados);    // sin movimiento de stock
        Assert.Equal(5m, producto.Existencia); // existencia intacta
    }

    // ──────────────────────────────────────────────
    // Mismo producto en 2 líneas: la suma supera el stock
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRechazar_CuandoSumaDeDosLineasExcedeExistencia()
    {
        // existencia 5; línea1=6 + línea2=4 = 10 > 5
        var producto = CrearProductoConStock(existencia: 5m);
        var docRepo = new FakeDocumentoVentaRepository();
        var movRepo = new FakeMovimientoStockRepository();
        var productoRepo = new FakeProductoRepository(producto);

        var handler = CrearHandler(productoRepo, docRepo, movRepo);
        // mismo productoId en 2 líneas
        var comando = ComandoFactura(producto.Id, cantidad: 6m, productoId2: producto.Id, cantidad2: 4m);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Equal(5m, producto.Existencia);
    }

    // ──────────────────────────────────────────────
    // Caso OK: cantidad <= existencia → factura se guarda
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeGuardarFactura_CuandoCantidadDentroDeExistencia()
    {
        var producto = CrearProductoConStock(existencia: 5m);
        var docRepo = new FakeDocumentoVentaRepository();
        var movRepo = new FakeMovimientoStockRepository();
        var productoRepo = new FakeProductoRepository(producto);

        var handler = CrearHandler(productoRepo, docRepo, movRepo);
        var comando = ComandoFactura(producto.Id, cantidad: 5m);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(docRepo.Actualizados);   // documento actualizado (Emitir)
        Assert.Single(movRepo.Guardados);      // movimiento de stock registrado
        Assert.Equal(0m, producto.Existencia); // existencia baja a 0
    }

    // ──────────────────────────────────────────────
    // Flag NoAplicaExistencias ON → factura sin validar stock
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeGuardarFactura_CuandoProductoNoAplicaExistencias()
    {
        var producto = CrearProductoConStock(existencia: 0m, noAplicaExistencias: true);
        var docRepo = new FakeDocumentoVentaRepository();
        var productoRepo = new FakeProductoRepository(producto);

        var handler = CrearHandler(productoRepo, docRepo);
        var comando = ComandoFactura(producto.Id, cantidad: 10m);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(docRepo.Actualizados);
    }

}
