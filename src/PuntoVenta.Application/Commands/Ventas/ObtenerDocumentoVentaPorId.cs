using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerDocumentoVentaPorIdQuery(Guid Id) : IRequest<ErrorOr<DocumentoVentaDto>>;

public sealed class ObtenerDocumentoVentaPorIdHandler(IDocumentoVentaRepository repository) : IRequestHandler<ObtenerDocumentoVentaPorIdQuery, ErrorOr<DocumentoVentaDto>>
{
    private readonly IDocumentoVentaRepository _repository = repository;

    public async ValueTask<ErrorOr<DocumentoVentaDto>> Handle(ObtenerDocumentoVentaPorIdQuery query, CancellationToken cancellationToken)
    {
        var documento = await _repository.ObtenerDetalleAsync(query.Id, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var documentosGenerados = await _repository.ObtenerDocumentosGeneradosAsync(query.Id, cancellationToken);
        var consumoNotas = await _repository.ObtenerConsumoNotasCreditoPorProductoAsync(query.Id, cancellationToken);
        // NCs emitidas contra cada generado (p. ej. una ND reversada por su NC):
        // permite mostrar el saldo vigente de la ND y que la factura salga
        // anulada cuando esa ND ya no tiene monto.
        var idsGenerados = documentosGenerados.Select(d => d.Id).ToList();
        var notasCreditoPorGenerado = await _repository.ObtenerMontoNotasCreditoPorDocumentosAsync(idsGenerados, cancellationToken);
        var notasCreditoDocumento = await _repository.ObtenerMontoNotasCreditoPorDocumentosAsync([documento.Id], cancellationToken);
        var notasDebitoDocumento = await _repository.ObtenerMontoNotasDebitoPorDocumentosAsync([documento.Id], cancellationToken);
        return DocumentoVentaMapper.ToDto(
            documento,
            documentosGenerados,
            consumoNotas,
            notasCreditoPorGenerado,
            notasCreditoDocumento.TryGetValue(documento.Id, out var montoNc) ? montoNc : 0m,
            notasDebitoDocumento.TryGetValue(documento.Id, out var montoNd) ? montoNd : 0m);
    }
}
