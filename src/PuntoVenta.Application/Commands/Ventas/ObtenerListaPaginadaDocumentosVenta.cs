using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerListaPaginadaDocumentosVentaQuery(
    int NumeroPagina = 1,
    int TamanoPagina = 10,
    string? FiltroDinamico = null,
    TipoDocumentoVenta? TipoDocumento = null,
    EstadoDocumentoVenta? Estado = null,
    Guid? ClienteId = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null) : IRequest<ErrorOr<PagedResult<DocumentoVentaResumenDto>>>;

public sealed class ObtenerListaPaginadaDocumentosVentaHandler(IDocumentoVentaRepository repository) : IRequestHandler<ObtenerListaPaginadaDocumentosVentaQuery, ErrorOr<PagedResult<DocumentoVentaResumenDto>>>
{
    private readonly IDocumentoVentaRepository _repository = repository;

    public async ValueTask<ErrorOr<PagedResult<DocumentoVentaResumenDto>>> Handle(ObtenerListaPaginadaDocumentosVentaQuery query, CancellationToken cancellationToken)
    {
        var (pagina, tamano) = Paginacion.Normalizar(query.NumeroPagina, query.TamanoPagina);

        var (items, total) = await _repository.ObtenerListaPaginadoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.TipoDocumento,
            query.Estado,
            query.ClienteId,
            query.FechaDesde,
            query.FechaHasta,
            cancellationToken);

        // Solo facturas y notas débito pueden tener NCs aplicadas.
        var facturaIds = items
            .Where(d => d.TipoDocumento is TipoDocumentoVenta.Factura or TipoDocumentoVenta.NotaDebito)
            .Select(d => d.Id)
            .ToList();
        var montosNotas = await _repository.ObtenerMontoNotasCreditoPorDocumentosAsync(facturaIds, cancellationToken);
        // Solo facturas pueden tener NDs (cargos adicionales) aplicadas.
        var notaDebitoIds = items
            .Where(d => d.TipoDocumento is TipoDocumentoVenta.Factura)
            .Select(d => d.Id)
            .ToList();
        var montosNotasDebito = await _repository.ObtenerMontoNotasDebitoPorDocumentosAsync(notaDebitoIds, cancellationToken);

        return PagedResult<DocumentoVentaResumenDto>.Crear(
            [.. items.Select(d => DocumentoVentaMapper.ToResumenDto(
                d,
                montosNotas.TryGetValue(d.Id, out var monto) ? monto : 0m,
                montosNotasDebito.TryGetValue(d.Id, out var montoND) ? montoND : 0m))],
            pagina, tamano, total);
    }
}
