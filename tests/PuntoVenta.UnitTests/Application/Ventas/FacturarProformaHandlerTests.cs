using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class FacturarProformaHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    // ──────────────────────────────────────────────
    // Stock insuficiente → rechaza, no persiste, no consume consecutivo
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRechazar_CuandoStockInsuficienteEnLineasDeProforma()
    {
        // Producto sin existencias (noAplicaExistencias=false, existencia=0).
        var producto = Producto.Crear("PRO-STK", "Prod Proforma", TipoItem.Bien, 800m,
            noAplicaExistencias: false).Value;

        var proforma = CrearProformaNumeradaConProducto(producto, cantidad: 10m);
        var docRepo = new FakeDocumentoVentaRepository(editable: proforma);
        var secRepo = new FakeSecuenciaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var productoRepo = new FakeProductoRepository(producto);

        var handler = CrearHandler(docRepo, secRepo, unitOfWork, productoRepo);
        var resultado = await handler.Handle(
            new FacturarProformaCommand(proforma.Id, [BuildPago(8000m)]),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Empty(docRepo.Guardados);               // factura NO persistida
        Assert.True(unitOfWork.LastTransaction.RolledBack); // consecutivo no queda
    }

    // ──────────────────────────────────────────────
    // Stock suficiente → emite y descuenta
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeEmitirFactura_CuandoStockSuficiente()
    {
        var producto = Producto.Crear("PRO-OK", "Prod Proforma OK", TipoItem.Bien, 100m,
            noAplicaExistencias: false).Value;
        producto.AplicarMovimientoStock(5m);

        var proforma = CrearProformaNumeradaConProducto(producto, cantidad: 5m);
        var docRepo = new FakeDocumentoVentaRepository(editable: proforma);
        var movRepo = new FakeMovimientoStockRepository();
        var handler = CrearHandler(docRepo, movimientoRepo: movRepo, productoRepo: new FakeProductoRepository(producto));

        var resultado = await handler.Handle(
            new FacturarProformaCommand(proforma.Id, [BuildPago(500m)]),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(docRepo.Guardados);                          // factura persistida
        Assert.Equal(EstadoDocumentoVenta.Convertido, proforma.Estado);
        Assert.Equal(0m, producto.Existencia);                     // existencia descontada
        Assert.NotEmpty(movRepo.Guardados);                        // movimiento creado
    }

    // ──────────────────────────────────────────────
    // noAplicaExistencias=true → no valida, pasa aunque existencia=0
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebePermitir_CuandoProductoNoAplicaExistencias()
    {
        // Bien con noAplicaExistencias=true (p.ej. activo que no se controla por stock)
        var producto = Producto.Crear("PRO-SRVK", "Bien Sin Existencias", TipoItem.Bien, 200m,
            noAplicaExistencias: true).Value;
        // existencia = 0, pero noAplicaExistencias=true → se saltea validación

        var proforma = CrearProformaNumeradaConProducto(producto, cantidad: 99m);
        var docRepo = new FakeDocumentoVentaRepository(editable: proforma);
        var handler = CrearHandler(docRepo, productoRepo: new FakeProductoRepository(producto));

        var resultado = await handler.Handle(
            new FacturarProformaCommand(proforma.Id, [BuildPago(19800m)]),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(docRepo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static DocumentoVenta CrearProformaNumeradaConProducto(Producto producto, decimal cantidad = 1m)
    {
        var doc = DocumentoVenta.Crear(
            TipoDocumentoVenta.Proforma,
            null, null,
            "01", "Contado",
            DominioHelper.FechaDocumento,
            "CRC", 1m).Value;

        doc.AgregarLinea(
            producto.Id,
            producto.TipoItem,
            producto.Codigo,
            producto.Nombre,
            "Unid",
            cantidad,
            producto.PrecioUnitario,
            noAplicaExistencias: producto.NoAplicaExistencias);

        doc.NumerarProforma(0, CajaId, "PRO-000001");
        return doc;
    }

    private static DocumentoVentaPagoCommand BuildPago(decimal monto)
        => new("CRC", 1m, "01", monto, monto, monto, 0m, 0m);

    private static FacturarProformaHandler CrearHandler(
        FakeDocumentoVentaRepository? docRepo = null,
        FakeSecuenciaRepository? secRepo = null,
        FakeUnitOfWork? unitOfWork = null,
        FakeProductoRepository? productoRepo = null,
        FakeMovimientoStockRepository? movimientoRepo = null)
        => new FacturarProformaHandler(
            unitOfWork ?? new FakeUnitOfWork(),
            docRepo ?? new FakeDocumentoVentaRepository(),
            new FakeMedioPagoRepository(DominioHelper.CrearMedioPago()),
            secRepo ?? new FakeSecuenciaRepository(),
            new FakeNegocioRepository(),
            new FakeDocumentoVentaEventoService(),
            productoRepo ?? new FakeProductoRepository(),
            movimientoRepo ?? new FakeMovimientoStockRepository(),
            new FakeFechaActual(),
            new FakeUsuarioActualVentas());
}
