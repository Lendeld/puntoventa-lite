using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record RegistrarAbonoFacturaCommand(
    Guid Id,
    DocumentoVentaPagoCommand Pago,
    DateTime? FechaPago = null) : IRequest<ErrorOr<Guid>>;

public sealed class RegistrarAbonoFacturaHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaRepository documentoRepository,
    IMedioPagoRepository medioPagoRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<RegistrarAbonoFacturaCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IMedioPagoRepository _medioPagoRepository = medioPagoRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(RegistrarAbonoFacturaCommand command, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        if (documento.TipoDocumento != TipoDocumentoVenta.Factura
            || documento.Estado != EstadoDocumentoVenta.Emitido
            || !documento.EsCredito)
        {
            return DocumentoVentaErrors.AbonoSoloEnCredito;
        }

        var pagos = await VentasHandlerHelpers.PrepararPagosAsync([command.Pago], _medioPagoRepository, cancellationToken);
        if (pagos.IsError)
        {
            return pagos.Errors;
        }

        var ahora = _fechaActual.AhoraUtc;
        var fechaPago = command.FechaPago.HasValue
            ? VentasHandlerHelpers.NormalizarFechaUtc(command.FechaPago)
            : ahora;

        var fechaPagoLocal = _fechaActual.ALocal(fechaPago);
        var fechaDocumentoLocal = _fechaActual.ALocal(documento.FechaDocumento);

        if (fechaPagoLocal > _fechaActual.Hoy)
        {
            return DocumentoVentaErrors.FechaPagoFutura;
        }

        if (fechaPagoLocal < fechaDocumentoLocal)
        {
            return DocumentoVentaErrors.FechaPagoAnteriorAlDocumento;
        }

        var montoNotasCredito = await _documentoRepository.ObtenerMontoNotasEmitidasAsync(
            documento.Id,
            TipoDocumentoVenta.NotaCredito,
            cancellationToken);

        var saldoReal = Dinero.RedondearPago(documento.SaldoPendiente - montoNotasCredito);
        if (saldoReal <= 0m)
        {
            return DocumentoVentaErrors.AbonoFacturaCubiertaPorNotaCredito;
        }

        var (pago, medioPago) = pagos.Value[0];
        if (Dinero.RedondearPago(pago.MontoAplicadoDocumento) > saldoReal)
        {
            return DocumentoVentaErrors.PagoExcedeSaldo;
        }

        var registrar = documento.RegistrarAbonoCredito(
            pago.MonedaCodigo,
            pago.TipoCambioAplicado,
            medioPago.Codigo,
            medioPago.Detalle,
            pago.MontoEntregado,
            pago.MontoAplicadoMonedaPago,
            pago.MontoAplicadoDocumento,
            pago.MontoVueltoMonedaPago,
            pago.MontoVueltoDocumento,
            fechaPago,
            ahora,
            ahora,
            _usuarioActual.UsuarioId,
            pago.Referencia,
            pago.Observacion);

        if (registrar.IsError)
        {
            return registrar.Errors;
        }

        var saldoCancelado = documento.SaldoPendiente == 0m;
        var tipo = saldoCancelado ? "SaldoCancelado" : "AbonoRegistrado";
        var resumen = saldoCancelado
            ? $"Saldo cancelado con abono de {pago.MontoAplicadoDocumento:N2} {documento.MonedaCodigo}"
            : $"Abono de {pago.MontoAplicadoDocumento:N2} {documento.MonedaCodigo} vía {medioPago.Detalle}";
        var saldoNeto = Math.Max(0m, Dinero.RedondearPago(documento.SaldoPendiente - montoNotasCredito));

        _ = await _eventoService.RegistrarAsync(
            documento.Id,
            tipo,
            resumen,
            payload: new
            {
                monto = pago.MontoAplicadoDocumento,
                moneda = documento.MonedaCodigo,
                medioPago = medioPago.Codigo,
                medioPagoDetalle = medioPago.Detalle,
                saldoPendiente = saldoNeto,
                totalPagado = documento.TotalPagado,
                referencia = pago.Referencia,
                fechaPago = registrar.Value.FechaPago,
                fechaRegistroUtc = registrar.Value.FechaRegistroUtc,
                pagoId = registrar.Value.Id,
                numeroAbono = registrar.Value.NumeroAbono
            },
            cancellationToken: cancellationToken);
        await _documentoRepository.RegistrarAbonoAsync(documento, registrar.Value, cancellationToken);
        return registrar.Value.Id;
    }
}
