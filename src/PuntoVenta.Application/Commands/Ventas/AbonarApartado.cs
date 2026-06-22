using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record AbonarApartadoCommand(
    Guid Id,
    DocumentoVentaPagoCommand Pago,
    DateTime? FechaPago = null) : IRequest<ErrorOr<Guid>>;

public sealed class AbonarApartadoHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaRepository documentoRepository,
    IMedioPagoRepository medioPagoRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<AbonarApartadoCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IMedioPagoRepository _medioPagoRepository = medioPagoRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(AbonarApartadoCommand command, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        if (documento.TipoDocumento != TipoDocumentoVenta.Apartado
            || (documento.Estado != EstadoDocumentoVenta.Reservado && documento.Estado != EstadoDocumentoVenta.Vencido))
        {
            return DocumentoVentaErrors.DocumentoNoEditable;
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
        var (pago, medioPago) = pagos.Value[0];
        var registrar = documento.RegistrarAbonoApartado(
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
            ? $"Saldo del apartado cancelado con abono de {pago.MontoAplicadoDocumento:N2} {documento.MonedaCodigo}"
            : $"Abono al apartado de {pago.MontoAplicadoDocumento:N2} {documento.MonedaCodigo} vía {medioPago.Detalle}";

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
                saldoPendiente = documento.SaldoPendiente,
                totalPagado = documento.TotalPagado,
                fechaPago = registrar.Value.FechaPago,
                fechaRegistroUtc = registrar.Value.FechaRegistroUtc,
                pagoId = registrar.Value.Id
            },
            cancellationToken: cancellationToken);
        await _documentoRepository.RegistrarAbonoAsync(documento, registrar.Value, cancellationToken);
        return documento.Id;
    }
}
