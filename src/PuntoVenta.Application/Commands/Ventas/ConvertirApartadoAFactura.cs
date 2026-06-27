using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ConvertirApartadoAFacturaCommand(Guid Id, Guid? CajaId = null) : IRequest<ErrorOr<Guid>>;

public sealed class ConvertirApartadoAFacturaHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IUnitOfWork unitOfWork,
    IDocumentoVentaRepository documentoRepository,
    ISecuenciaRepository secuenciaRepository,
    INegocioRepository negocioRepository,
    IDocumentoVentaEventoService eventoService,
    IProductoRepository productoRepository,
    IMovimientoStockRepository movimientoStockRepository) : IRequestHandler<ConvertirApartadoAFacturaCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly ISecuenciaRepository _secuenciaRepository = secuenciaRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly IMovimientoStockRepository _movimientoStockRepository = movimientoStockRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(ConvertirApartadoAFacturaCommand command, CancellationToken cancellationToken)
    {
        var apartado = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (apartado is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        if (apartado.TipoDocumento != TipoDocumentoVenta.Apartado
            || (apartado.Estado != EstadoDocumentoVenta.Reservado && apartado.Estado != EstadoDocumentoVenta.Vencido))
        {
            return DocumentoVentaErrors.DocumentoNoConvertible;
        }

        if (apartado.SaldoPendiente > 0)
        {
            return DocumentoVentaErrors.ApartadoConSaldoPendiente;
        }

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        Guid? cajaId = negocio?.AplicaCajas == true ? command.CajaId : null;

        await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var consecutivo = await VentasHandlerHelpers.SiguienteConsecutivoAsync(
            TipoDocumentoVenta.Factura,
            _secuenciaRepository,
            cancellationToken);
        if (consecutivo.IsError)
        {
            await tx.RollbackAsync(cancellationToken);
            return consecutivo.Errors;
        }

        var factura = DocumentoVenta.Crear(
            TipoDocumentoVenta.Factura,
            apartado.ClienteId,
            apartado.VendedorId,
            apartado.CondicionVentaCodigo,
            apartado.CondicionVentaDetalleSnapshot,
            DateTime.UtcNow,
            apartado.MonedaCodigo,
            apartado.TipoCambio,
            null,
            $"Factura generada desde apartado {apartado.Consecutivo}",
            documentoOrigenId: apartado.Id);

        if (factura.IsError)
        {
            return factura.Errors;
        }

        foreach (var linea in apartado.Lineas)
        {
            var agregar = factura.Value.AgregarLinea(
                linea.ProductoId,
                linea.TipoItem,
                linea.Codigo,
                linea.Descripcion,
                linea.UnidadMedidaCodigo,
                linea.Cantidad,
                linea.PrecioUnitario,
                linea.MontoDescuento,
                linea.TarifaIvaImpuestoCodigo,
                linea.PorcentajeImpuesto,
                false,
                linea.NoAplicaExistencias,
                linea.PermiteModificarPrecioUnitario);
            if (agregar.IsError)
            {
                return agregar.Errors;
            }
        }

        foreach (var pago in ConsolidarPagos(apartado.Pagos))
        {
            var agregar = factura.Value.AgregarPago(
                pago.MonedaCodigo,
                pago.TipoCambioAplicado,
                pago.MedioPagoCodigo,
                pago.MedioPagoDetalleSnapshot,
                pago.MontoEntregado,
                pago.MontoAplicadoMonedaPago,
                pago.MontoAplicadoDocumento,
                pago.MontoVueltoMonedaPago,
                pago.MontoVueltoDocumento,
                null,
                $"Consolidado de abonos del apartado {apartado.Consecutivo}",
                pago.FechaPago,
                _usuarioActual.UsuarioId);
            if (agregar.IsError)
            {
                return agregar.Errors;
            }
        }

        var emitir = factura.Value.Emitir(cajaId, consecutivo.Value);
        if (emitir.IsError)
        {
            return emitir.Errors;
        }

        var convertir = apartado.MarcarConvertido();
        if (convertir.IsError)
        {
            return convertir.Errors;
        }

        var correlacionId = Guid.NewGuid();

        _ = await _eventoService.RegistrarAsync(
            apartado.Id,
            "ApartadoConvertidoAFactura",
            $"Apartado {apartado.Consecutivo} convertido a factura {factura.Value.Consecutivo}",
            payload: new
            {
                apartadoConsecutivo = apartado.Consecutivo,
                facturaConsecutivo = factura.Value.Consecutivo,
                facturaId = factura.Value.Id,
                total = factura.Value.TotalComprobante,
                moneda = factura.Value.MonedaCodigo
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);

        _ = await _eventoService.RegistrarAsync(
            factura.Value.Id,
            "FacturaEmitida",
            $"Factura {factura.Value.Consecutivo} emitida desde apartado {apartado.Consecutivo}",
            payload: new
            {
                consecutivo = factura.Value.Consecutivo,
                origenApartadoId = apartado.Id,
                origenApartadoConsecutivo = apartado.Consecutivo,
                total = factura.Value.TotalComprobante,
                moneda = factura.Value.MonedaCodigo
            },
            correlacionId: correlacionId,
            cancellationToken: cancellationToken);
        var stock = await VentasHandlerHelpers.AplicarMovimientosStockAsync(
            factura.Value.Lineas,
            factura.Value,
            deltaEsNegativo: true,
            _productoRepository,
            _movimientoStockRepository,
            _fechaActual.AhoraUtc,
            _usuarioActual.UsuarioId,
            cancellationToken);
        if (stock.IsError)
        {
            await tx.RollbackAsync(cancellationToken);
            return stock.Errors;
        }

        await _documentoRepository.AddAsync(factura.Value, cancellationToken);
        await _documentoRepository.UpdateAsync(apartado, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return factura.Value.Id;
    }

    private static IEnumerable<PagoConsolidado> ConsolidarPagos(IEnumerable<DocumentoVentaPago> pagos)
        => pagos
            .GroupBy(p => new
            {
                p.MedioPagoCodigo,
                p.MedioPagoDetalleSnapshot,
                p.MonedaCodigo,
                p.TipoCambioAplicado
            })
            .Select(g => new PagoConsolidado(
                g.Key.MedioPagoCodigo,
                g.Key.MedioPagoDetalleSnapshot,
                g.Key.MonedaCodigo,
                g.Key.TipoCambioAplicado,
                g.Sum(x => x.MontoEntregado),
                g.Sum(x => x.MontoAplicadoMonedaPago),
                g.Sum(x => x.MontoAplicadoDocumento),
                g.Sum(x => x.MontoVueltoMonedaPago),
                g.Sum(x => x.MontoVueltoDocumento),
                g.Min(x => x.FechaPago)));

    private sealed record PagoConsolidado(
        string MedioPagoCodigo,
        string MedioPagoDetalleSnapshot,
        string MonedaCodigo,
        decimal TipoCambioAplicado,
        decimal MontoEntregado,
        decimal MontoAplicadoMonedaPago,
        decimal MontoAplicadoDocumento,
        decimal MontoVueltoMonedaPago,
        decimal MontoVueltoDocumento,
        DateTime FechaPago);
}
