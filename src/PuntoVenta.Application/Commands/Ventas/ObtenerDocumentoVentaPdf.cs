using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerDocumentoVentaPdfQuery(Guid Id) : IRequest<ErrorOr<DocumentoVentaPdfDto>>;

public sealed class ObtenerDocumentoVentaPdfHandler(
    IDocumentoVentaRepository documentoRepository,
    INegocioRepository negocioRepository,
    INegocioTicketConfigRepository ticketConfigRepository,
    IDocumentoVentaPdfService pdfService) : IRequestHandler<ObtenerDocumentoVentaPdfQuery, ErrorOr<DocumentoVentaPdfDto>>
{
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly INegocioTicketConfigRepository _ticketConfigRepository = ticketConfigRepository;
    private readonly IDocumentoVentaPdfService _pdfService = pdfService;

    public async ValueTask<ErrorOr<DocumentoVentaPdfDto>> Handle(ObtenerDocumentoVentaPdfQuery query, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.ObtenerDetalleAsync(query.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        if (negocio is null)
        {
            return Error.NotFound("Negocio.NoEncontrado", "No se encontró el negocio.");
        }

        var ticketConfig = await _ticketConfigRepository.ObtenerAsync(cancellationToken);

        var content = await _pdfService.GenerarPdfAsync(documento, negocio, ticketConfig, cancellationToken);
        var label = string.IsNullOrWhiteSpace(documento.Consecutivo)
            ? $"borrador-{documento.Id.ToString("N")[..8]}"
            : documento.Consecutivo;
        var prefijo = documento.TipoDocumento == TipoDocumentoVenta.Proforma ? "proforma" : "venta";
        var fileName = $"{prefijo}-{label}.pdf";

        return new DocumentoVentaPdfDto(content, fileName, "application/pdf");
    }
}
