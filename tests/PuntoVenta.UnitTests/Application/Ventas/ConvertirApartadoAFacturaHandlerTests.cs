using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Ventas;
using PuntoVenta.UnitTests.Application.Ventas.Fakes;

namespace PuntoVenta.UnitTests.Application.Ventas;

public class ConvertirApartadoAFacturaHandlerTests
{
    private static readonly Guid CajaId = DominioHelper.CajaId;

    // ──────────────────────────────────────────────
    // Éxito — apartado reservado con saldo cero
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeCrearFactura_CuandoApartadoReservadoSaldoCero()
    {
        var apartado = DominioHelper.CrearApartadoReservado(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: apartado);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ConvertirApartadoAFacturaCommand(apartado.Id),
            CancellationToken.None);

        Assert.False(resultado.IsError);
        Assert.Single(repo.Guardados); // factura creada
        Assert.Single(repo.Actualizados); // apartado marcado convertido
        Assert.Equal(EstadoDocumentoVenta.Convertido, apartado.Estado);
    }

    // ──────────────────────────────────────────────
    // El consecutivo de la factura generada sigue formato FAC-NNNNNN
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeAsignarConsecutivoFactura_CuandoConvierteApartado()
    {
        var apartado = DominioHelper.CrearApartadoReservado(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: apartado);
        var handler = CrearHandler(repo);

        await handler.Handle(new ConvertirApartadoAFacturaCommand(apartado.Id), CancellationToken.None);

        var facturaCreada = repo.Guardados.Single();
        Assert.StartsWith("FAC-", facturaCreada.Consecutivo);
    }

    // ──────────────────────────────────────────────
    // Apartado no encontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoApartadoNoExiste()
    {
        var repo = new FakeDocumentoVentaRepository(editable: null);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ConvertirApartadoAFacturaCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.NoEncontrado.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Documento no es apartado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRetornarError_CuandoDocumentoEsFactura()
    {
        var factura = DominioHelper.CrearFacturaEmitida(CajaId);
        var repo = new FakeDocumentoVentaRepository(editable: factura);
        var handler = CrearHandler(repo);

        var resultado = await handler.Handle(
            new ConvertirApartadoAFacturaCommand(factura.Id),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == DocumentoVentaErrors.DocumentoNoConvertible.Code);
        Assert.Empty(repo.Guardados);
    }

    // ──────────────────────────────────────────────
    // Helper
    // ──────────────────────────────────────────────

    private static ConvertirApartadoAFacturaHandler CrearHandler(FakeDocumentoVentaRepository repo)
        => new ConvertirApartadoAFacturaHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            repo,
            new FakeSecuenciaRepository(),
            new FakeNegocioRepository(),
            new FakeDocumentoVentaEventoService(),
            new FakeProductoRepository(),
            new FakeMovimientoStockRepository());
}
