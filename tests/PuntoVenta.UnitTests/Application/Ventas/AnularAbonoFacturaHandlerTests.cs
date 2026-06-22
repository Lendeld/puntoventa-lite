using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class AnularAbonoFacturaHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    [Fact]
    public async Task Handle_DebeAnularAbonoYRestaurarSaldo()
    {
        var factura = CrearFacturaConAbono(out var pago);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var usuario = new FakeUsuarioActualVentas(Guid.Parse("11111111-1111-1111-1111-111111111111"), "ana");
        var fechaActual = new FakeFechaActual(new DateTime(2024, 6, 5, 15, 0, 0, DateTimeKind.Utc));
        var eventoService = new FakeDocumentoVentaEventoService();
        var handler = new AnularAbonoFacturaHandler(usuario, fechaActual, repo, eventoService);

        var resultado = await handler.Handle(
            new AnularAbonoFacturaCommand(factura.Id, pago.Id, "Cliente solicita reversa"),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Equal(pago.Id, resultado.Value);
        var pagoAnulado = Assert.Single(repo.AbonosAnulados);
        Assert.True(pagoAnulado.Anulado);
        Assert.Equal(fechaActual.AhoraUtc, pagoAnulado.FechaAnulacionUtc);
        Assert.Equal(usuario.UsuarioId, pagoAnulado.UsuarioAnulaId);
        Assert.Equal("Cliente solicita reversa", pagoAnulado.MotivoAnulacion);
        Assert.Equal(factura.TotalComprobante, factura.SaldoPendiente);
        Assert.Equal(0m, factura.TotalPagado);
        Assert.Null(factura.FechaCancelacion);
        Assert.Equal("AbonoRevertido", Assert.Single(eventoService.EventosRegistrados).TipoEventoCodigo);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoMotivoEsInvalido()
    {
        var factura = CrearFacturaConAbono(out var pago);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = new AnularAbonoFacturaHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeDocumentoVentaEventoService());

        var resultado = await handler.Handle(
            new AnularAbonoFacturaCommand(factura.Id, pago.Id, "   "),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.MotivoAnulacionRequerido.Code);
        Assert.Empty(repo.AbonosAnulados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPagoNoExiste()
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = new AnularAbonoFacturaHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeDocumentoVentaEventoService());

        var resultado = await handler.Handle(
            new AnularAbonoFacturaCommand(factura.Id, Guid.NewGuid(), "No corresponde"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.NoEncontrado.Code);
        Assert.Empty(repo.AbonosAnulados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoPagoYaFueAnulado()
    {
        var factura = CrearFacturaConAbono(out var pago);
        factura.AnularAbono(pago.Id, Guid.NewGuid(), "Primera anulación", DateTime.UtcNow);

        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = new AnularAbonoFacturaHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeDocumentoVentaEventoService());

        var resultado = await handler.Handle(
            new AnularAbonoFacturaCommand(factura.Id, pago.Id, "Segunda anulación"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.YaAnulado.Code);
        Assert.Empty(repo.AbonosAnulados);
    }

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoUsuarioEsRequerido()
    {
        var factura = CrearFacturaConAbono(out var pago);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = new AnularAbonoFacturaHandler(
            new FakeUsuarioActualVentas(Guid.Empty),
            new FakeFechaActual(),
            repo,
            new FakeDocumentoVentaEventoService());

        var resultado = await handler.Handle(
            new AnularAbonoFacturaCommand(factura.Id, pago.Id, "Motivo válido"),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaPagoErrors.UsuarioAnulaInvalido.Code);
        Assert.Empty(repo.AbonosAnulados);
    }

    private static DocumentoVenta CrearFacturaConAbono(out DocumentoVentaPago pago)
    {
        var factura = DominioHelper.CrearFacturaCreditoEmitida(CajaId);
        var registrar = factura.RegistrarAbonoCredito(
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
            null);

        pago = registrar.Value;
        return factura;
    }
}
