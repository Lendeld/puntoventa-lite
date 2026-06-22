using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class ObtenerTicketDataHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    [Fact]
    public async Task Handle_DebeConstruirReciboDeFacturaCreditoActiva()
    {
        var factura = CrearFacturaCreditoConAbono(out var pago);
        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ObtenerTicketDataQuery(factura.Id, pago.Id),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.EsRecibo);
        Assert.False(resultado.Value.EsReciboAnulado);
        Assert.Empty(resultado.Value.Lineas);
        Assert.Single(resultado.Value.Pagos);
        Assert.Equal(1, resultado.Value.Pagos[0].NumeroAbono);
        Assert.Equal(1000m, resultado.Value.SaldoAnterior);
        Assert.Equal(600m, resultado.Value.SaldoNuevo);
        Assert.Equal(600m, resultado.Value.Saldo);
    }

    [Fact]
    public async Task Handle_DebeConstruirReciboAnuladoConSaldoRestaurado()
    {
        var factura = CrearFacturaCreditoConAbono(out var pago);
        factura.AnularAbono(
            pago.Id,
            Guid.NewGuid(),
            "Se anuló el recibo",
            DominioHelper.FechaDocumento.AddHours(2));

        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ObtenerTicketDataQuery(factura.Id, pago.Id),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.True(resultado.Value.EsRecibo);
        Assert.True(resultado.Value.EsReciboAnulado);
        Assert.Equal(600m, resultado.Value.SaldoAnterior);
        Assert.Equal(1000m, resultado.Value.SaldoNuevo);
        Assert.Equal("Se anuló el recibo", resultado.Value.MotivoAnulacion);
        Assert.Equal(1000m, resultado.Value.Saldo);
    }

    [Fact]
    public async Task Handle_DebeOmitirPagosAnuladosEnTicketCompleto()
    {
        var factura = CrearFacturaCreditoConAbono(out var pago);
        factura.AnularAbono(
            pago.Id,
            Guid.NewGuid(),
            "Reversa",
            DominioHelper.FechaDocumento.AddHours(2));

        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ObtenerTicketDataQuery(factura.Id, null),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.False(resultado.Value.EsRecibo);
        Assert.Empty(resultado.Value.Pagos);
        Assert.Single(resultado.Value.Lineas);
    }

    [Fact]
    public async Task Handle_DebeRechazarReciboDeFacturaContado()
    {
        var factura = DominioHelper.CrearFacturaEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(detalle: factura);
        var handler = CrearHandler(repo);
        var pago = Assert.Single(factura.Pagos);

        var resultado = await handler.Handle(
            new ObtenerTicketDataQuery(factura.Id, pago.Id),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoEmiteReciboAbono.Code);
    }

    private static ObtenerTicketDataHandler CrearHandler(FakeDocumentoVentaRepository repo)
        => new(
            repo,
            new FakeNegocioRepository(),
            new FakeNegocioTicketConfigRepository());

    private static DocumentoVenta CrearFacturaCreditoConAbono(out DocumentoVentaPago pago)
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        pago = factura.RegistrarAbonoCredito(
            "CRC",
            1m,
            "01",
            "Efectivo",
            400m,
            400m,
            400m,
            0m,
            0m,
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            DominioHelper.FechaDocumento.AddHours(1),
            Guid.NewGuid(),
            null,
            null).Value;
        return factura;
    }
}
