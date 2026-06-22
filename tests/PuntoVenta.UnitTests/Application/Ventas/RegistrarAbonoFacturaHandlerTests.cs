using System.Text.Json;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class RegistrarAbonoFacturaHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    [Fact]
    public async Task Handle_DebeRetornarPagoIdYUsarFechaActual_CuandoFechaPagoNoSeEnvia()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var medio = DominioHelper.CrearMedioPago();
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var fechaActual = new FakeFechaActual(new DateTime(2024, 6, 5, 13, 15, 0, DateTimeKind.Utc));
        var eventoService = new FakeDocumentoVentaEventoService();
        var handler = CrearHandler(repo, medio, fechaActual, eventoService);

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 500m, 500m, 500m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(factura.Id, pago),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        var pagoRegistrado = Assert.Single(repo.AbonosRegistrados);
        Assert.Equal(pagoRegistrado.Id, resultado.Value);
        Assert.Equal(fechaActual.AhoraUtc, pagoRegistrado.FechaPago);
        Assert.Equal(fechaActual.AhoraUtc, pagoRegistrado.FechaRegistroUtc);
        Assert.Equal(1, pagoRegistrado.NumeroAbono);
        Assert.Null(pagoRegistrado.Referencia);

        var evento = Assert.Single(eventoService.EventosRegistrados);
        Assert.Equal("AbonoRegistrado", evento.TipoEventoCodigo);

        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(evento.Payload));
        Assert.Equal(pagoRegistrado.Id, payload.RootElement.GetProperty("pagoId").GetGuid());
        Assert.Equal(1, payload.RootElement.GetProperty("numeroAbono").GetInt32());
        Assert.Equal(500m, payload.RootElement.GetProperty("saldoPendiente").GetDecimal());
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoDocumentoNoExiste()
    {
        var repo = new FakeDocumentoVentaRepository(editable: null);
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago());

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 500m, 500m, 500m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(Guid.NewGuid(), pago),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.NoEncontrado.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoFacturaContado()
    {
        var facturaContado = DominioHelper.CrearFacturaEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: facturaContado);
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago());

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 500m, 500m, 500m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(facturaContado.Id, pago),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.AbonoSoloEnCredito.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoFechaPagoEsFutura()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var fechaActual = new FakeFechaActual(new DateTime(2024, 6, 5, 13, 15, 0, DateTimeKind.Utc));
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago(), fechaActual);

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 200m, 200m, 200m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(
                factura.Id,
                pago,
                FechaPago: fechaActual.AhoraUtc.AddDays(1)),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.FechaPagoFutura.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoFechaPagoEsAnteriorAlDocumento()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago());

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 200m, 200m, 200m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(
                factura.Id,
                pago,
                FechaPago: DominioHelper.FechaDocumento.AddDays(-1)),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.FechaPagoAnteriorAlDocumento.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoNotaCreditoCubreSaldo()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: factura)
        {
            MontoNotasEmitidas = factura.TotalComprobante
        };
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago());

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 200m, 200m, 200m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(factura.Id, pago),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.AbonoFacturaCubiertaPorNotaCredito.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoMontoExcedeSaldoNetoReal()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: factura)
        {
            MontoNotasEmitidas = 700m
        };
        var handler = CrearHandler(repo, DominioHelper.CrearMedioPago());

        var pago = new DocumentoVentaPagoCommand("CRC", 1m, "01", 400m, 400m, 400m, 0m, 0m);
        var resultado = await handler.Handle(
            new RegistrarAbonoFacturaCommand(factura.Id, pago),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.PagoExcedeSaldo.Code);
        Assert.Empty(repo.AbonosRegistrados);
    }

    private static RegistrarAbonoFacturaHandler CrearHandler(
        FakeDocumentoVentaRepository repo,
        MedioPago medio,
        FakeFechaActual? fechaActual = null,
        FakeDocumentoVentaEventoService? eventoService = null,
        FakeUsuarioActualVentas? usuarioActual = null)
        => new(
            usuarioActual ?? new FakeUsuarioActualVentas(),
            fechaActual ?? new FakeFechaActual(),
            repo,
            new FakeMedioPagoRepository(medio),
            eventoService ?? new FakeDocumentoVentaEventoService());
}
