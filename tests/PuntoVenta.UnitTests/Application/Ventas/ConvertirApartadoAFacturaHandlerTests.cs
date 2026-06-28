using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Domain.Entities.Productos;
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
    // Stock insuficiente — no persiste factura y NO consume consecutivo
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_DebeRechazar_CuandoStockInsuficienteEnLineasDelApartado()
    {
        // Producto con control de existencia (noAplicaExistencias=false) y 0 unidades.
        var producto = Producto.Crear("APT-STK", "Prod Apartado", TipoItem.Bien, 500m,
            tarifaIvaImpuestoCodigo: "08",
            noAplicaExistencias: false).Value;
        // existencia = 0

        var apartado = DominioHelper.CrearApartadoReservadoConProducto(CajaId, producto);
        var docRepo = new FakeDocumentoVentaRepository(editable: apartado);
        var secRepo = new FakeSecuenciaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CrearHandler(docRepo, new FakeProductoRepository(producto), secRepo, unitOfWork);

        var resultado = await handler.Handle(
            new ConvertirApartadoAFacturaCommand(apartado.Id),
            CancellationToken.None);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Empty(docRepo.Guardados);    // factura NO persistida
        Assert.True(unitOfWork.LastTransaction.RolledBack); // transacción revertida → consecutivo no queda
    }

    // ──────────────────────────────────────────────
    // Helper
    // ──────────────────────────────────────────────

    private static ConvertirApartadoAFacturaHandler CrearHandler(
        FakeDocumentoVentaRepository repo,
        FakeProductoRepository? productoRepo = null,
        FakeSecuenciaRepository? secRepo = null,
        FakeUnitOfWork? unitOfWork = null)
        => new ConvertirApartadoAFacturaHandler(
            new FakeUsuarioActualVentas(),
            new FakeFechaActual(),
            unitOfWork ?? new FakeUnitOfWork(),
            repo,
            secRepo ?? new FakeSecuenciaRepository(),
            new FakeNegocioRepository(),
            new FakeDocumentoVentaEventoService(),
            productoRepo ?? new FakeProductoRepository(),
            new FakeMovimientoStockRepository());
}
