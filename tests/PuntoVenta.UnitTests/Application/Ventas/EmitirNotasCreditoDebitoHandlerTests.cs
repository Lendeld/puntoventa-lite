using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class EmitirNotasCreditoDebitoHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    [Fact]
    public async Task EmitirNotaCredito_DebeBloquearFacturaCreditoConAbonoActivo()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        factura.RegistrarAbonoCredito(
            "CRC",
            1m,
            "01",
            "Efectivo",
            200m,
            200m,
            200m,
            0m,
            0m,
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            Guid.NewGuid(),
            null,
            null);

        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandlerNotaCredito(repo);

        var resultado = await handler.Handle(
            new EmitirNotaCreditoCommand(factura.Id, ModoNotaCredito.Anulacion, []),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.NotaCreditoSobreFacturaConAbonosActivos.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task EmitirNotaCredito_DebePermitirFacturaCreditoSinAbonosActivos()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var producto = DominioHelper.CrearProducto(1000m);
        var abono = factura.RegistrarAbonoCredito(
            "CRC",
            1m,
            "01",
            "Efectivo",
            200m,
            200m,
            200m,
            0m,
            0m,
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            Guid.NewGuid(),
            null,
            null).Value;
        factura.AnularAbono(abono.Id, Guid.NewGuid(), "Reversa previa", DominioHelper.FechaDocumento.AddHours(2));

        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandlerNotaCredito(repo, producto);

        var resultado = await handler.Handle(
            new EmitirNotaCreditoCommand(factura.Id, ModoNotaCredito.Anulacion, []),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados);
        Assert.Equal(TipoDocumentoVenta.NotaCredito, repo.Guardados[0].TipoDocumento);
    }

    [Fact]
    public async Task EmitirNotaDebito_DebeBloquearFacturaCredito()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandlerNotaDebito(repo);

        var resultado = await handler.Handle(
            new EmitirNotaDebitoCommand(factura.Id, []),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.NotaDebitoSobreFacturaCredito.Code);
        Assert.Empty(repo.Guardados);
    }

    [Fact]
    public async Task EmitirNotaDebito_DebePermitirFacturaContado()
    {
        var factura = DominioHelper.CrearFacturaEmitida(CajaId);
        var producto = DominioHelper.CrearProducto(150m);
        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandlerNotaDebito(repo, producto);

        var resultado = await handler.Handle(
            new EmitirNotaDebitoCommand(
                factura.Id,
                [
                    new DocumentoVentaLineaCommand(
                        producto.Id,
                        1m,
                        PrecioUnitario: 150m,
                        MontoDescuento: 0m,
                        Descripcion: "Cargo adicional")
                ]),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados);
        Assert.Equal(TipoDocumentoVenta.NotaDebito, repo.Guardados[0].TipoDocumento);
    }

    // ──────────────────────────────────────────────
    // NC reintegro con existencia 0 NO bloquea (deltaEsNegativo=false)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EmitirNotaCredito_Devolucion_NoBloquea_CuandoExistenciaEsCero()
    {
        // Producto con existencia 0 (ya sin stock) — el reintegro de NC no valida stock
        // (deltaEsNegativo=false), así que debe completarse sin error.
        var producto = Producto.Crear("P-REI", "Reintegro Test", TipoItem.Bien, 1000m,
            tarifaIvaImpuestoCodigo: "08",
            noAplicaExistencias: false).Value;
        // existencia queda en 0

        // Construir una factura emitida válida con ese producto como origen de la NC.
        var origen = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            null, null,
            "01", "Contado",
            DominioHelper.FechaDocumento,
            "CRC", 1m).Value;
        origen.AgregarLinea(producto.Id, TipoItem.Bien, "P-REI", "Reintegro Test",
            "Unid", 1m, 1000m);
        origen.AgregarPago("CRC", 1m, "01", "Efectivo", 1000m, 1000m, 1000m, 0m, 0m);
        origen.Emitir(CajaId, "FAC-REI-001");

        var repo = new FakeDocumentoVentaRepository(detalle: origen);
        var movRepo = new FakeMovimientoStockRepository();
        var handler = new EmitirNotaCreditoHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeSecuenciaRepository(),
            new FakeProductoRepository(producto),
            new FakeTarifaRepository(),
            new FakeDocumentoVentaEventoService(),
            movRepo);

        var resultado = await handler.Handle(
            new EmitirNotaCreditoCommand(
                origen.Id,
                ModoNotaCredito.Devolucion,
                [new DocumentoVentaLineaCommand(producto.Id, 1m, 1000m, 0m, DevuelveInventario: true)]),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados); // NC creada
        Assert.Single(movRepo.Guardados); // reintegro aplicado
        Assert.Equal(1m, producto.Existencia); // volvió 1 unidad
    }

    private static EmitirNotaCreditoHandler CrearHandlerNotaCredito(
        FakeDocumentoVentaRepository repo,
        params Producto[] productos)
        => new(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeSecuenciaRepository(),
            new FakeProductoRepository(productos),
            new FakeTarifaRepository(),
            new FakeDocumentoVentaEventoService(),
            new FakeMovimientoStockRepository());

    private static EmitirNotaDebitoHandler CrearHandlerNotaDebito(
        FakeDocumentoVentaRepository repo,
        params Producto[] productos)
        => new(
            repo,
            new FakeSecuenciaRepository(),
            new FakeProductoRepository(productos),
            new FakeTarifaRepository(),
            new FakeDocumentoVentaEventoService());
}
