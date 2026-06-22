using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerReciboPagoPdfQuery(Guid Id, Guid PagoId) : IRequest<ErrorOr<DocumentoVentaPdfDto>>;

public sealed class ObtenerReciboPagoPdfHandler(
    IDocumentoVentaRepository documentoRepository,
    INegocioRepository negocioRepository,
    INegocioTicketConfigRepository ticketConfigRepository,
    IDocumentoVentaPdfService pdfService) : IRequestHandler<ObtenerReciboPagoPdfQuery, ErrorOr<DocumentoVentaPdfDto>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly INegocioTicketConfigRepository _ticketConfigRepository = ticketConfigRepository;
    private readonly IDocumentoVentaPdfService _pdfService = pdfService;

    public async ValueTask<ErrorOr<DocumentoVentaPdfDto>> Handle(ObtenerReciboPagoPdfQuery query, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerDetalleAsync(query.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var tipoAceptado = documento.TipoDocumento == TipoDocumentoVenta.Apartado
            || (documento.TipoDocumento == TipoDocumentoVenta.Factura && documento.EsCredito);
        if (!tipoAceptado)
        {
            return DocumentoVentaErrors.DocumentoNoEmiteReciboAbono;
        }

        var pago = documento.Pagos.FirstOrDefault(p => p.Id == query.PagoId);
        if (pago is null)
        {
            return DocumentoVentaErrors.PagoNoEncontrado;
        }

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        if (negocio is null)
        {
            return Error.NotFound("Negocio_NoEncontrado", "No se encontró el negocio.");
        }

        var ticketConfig = await _ticketConfigRepository.ObtenerAsync(cancellationToken);
        var montoNotasCredito = documento.TipoDocumento == TipoDocumentoVenta.Factura
            ? await _documentoRepository.ObtenerMontoNotasEmitidasAsync(documento.Id, TipoDocumentoVenta.NotaCredito, cancellationToken)
            : 0m;
        var content = await _pdfService.GenerarReciboPagoPdfAsync(
            documento,
            pago,
            negocio,
            ticketConfig,
            montoNotasCredito,
            cancellationToken);
        var label = documento.Consecutivo ?? documento.Id.ToString("N")[..8];
        return new DocumentoVentaPdfDto(content, $"recibo-abono-{label}-{pago.Id.ToString("N")[..8]}.pdf", "application/pdf");
    }
}
