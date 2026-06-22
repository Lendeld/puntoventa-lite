using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record AnularAbonoFacturaCommand(
    Guid DocumentoId,
    Guid PagoId,
    string Motivo) : IRequest<ErrorOr<Guid>>;

public sealed class AnularAbonoFacturaHandler(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaRepository documentoRepository,
    IDocumentoVentaEventoService eventoService) : IRequestHandler<AnularAbonoFacturaCommand, ErrorOr<Guid>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IDocumentoVentaEventoService _eventoService = eventoService;

    public async ValueTask<ErrorOr<Guid>> Handle(AnularAbonoFacturaCommand command, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerEditableAsync(command.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var pago = documento.Pagos.FirstOrDefault(p => p.Id == command.PagoId);
        if (pago is null)
        {
            return DocumentoVentaPagoErrors.NoEncontrado;
        }

        if (_usuarioActual.UsuarioId == Guid.Empty)
        {
            return DocumentoVentaPagoErrors.UsuarioAnulaInvalido;
        }

        var ahora = _fechaActual.AhoraUtc;
        var anular = documento.AnularAbono(command.PagoId, _usuarioActual.UsuarioId, command.Motivo, ahora);
        if (anular.IsError)
        {
            return anular.Errors;
        }

        var pagoAnulado = anular.Value;

        _ = await _eventoService.RegistrarAsync(
            documento.Id,
            "AbonoRevertido",
            $"Abono de {pagoAnulado.MontoAplicadoDocumento:N2} {documento.MonedaCodigo} anulado. Motivo: {command.Motivo}",
            payload: new
            {
                monto = pagoAnulado.MontoAplicadoDocumento,
                moneda = documento.MonedaCodigo,
                medioPago = pagoAnulado.MedioPagoCodigo,
                medioPagoDetalle = pagoAnulado.MedioPagoDetalleSnapshot,
                saldoPendiente = documento.SaldoPendiente,
                totalPagado = documento.TotalPagado,
                motivo = pagoAnulado.MotivoAnulacion,
                usuarioAnula = _usuarioActual.NombreUsuario,
                pagoId = pagoAnulado.Id,
                numeroAbono = pagoAnulado.NumeroAbono,
                fechaPago = pagoAnulado.FechaPago,
                fechaRegistroUtc = pagoAnulado.FechaRegistroUtc,
                fechaAnulacionUtc = pagoAnulado.FechaAnulacionUtc
            },
            cancellationToken: cancellationToken);

        await _documentoRepository.AnularAbonoAsync(documento, pagoAnulado, cancellationToken);
        return pagoAnulado.Id;
    }
}
