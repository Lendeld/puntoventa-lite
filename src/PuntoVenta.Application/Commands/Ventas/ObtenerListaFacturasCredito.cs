using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerListaFacturasCreditoQuery(
    int Pagina = 1,
    int Tamano = 20,
    string? FiltroDinamico = null,
    Guid? ClienteId = null,
    bool? SoloVencidas = null) : IRequest<ErrorOr<PagedResult<FacturaCreditoResumenDto>>>;

public sealed class ObtenerListaFacturasCreditoHandler(IFechaActual fechaActual, IDocumentoVentaRepository documentoRepository) : IRequestHandler<ObtenerListaFacturasCreditoQuery, ErrorOr<PagedResult<FacturaCreditoResumenDto>>>
{
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;

    public async ValueTask<ErrorOr<PagedResult<FacturaCreditoResumenDto>>> Handle(ObtenerListaFacturasCreditoQuery query, CancellationToken cancellationToken)
    {
        var pagina = query.Pagina < 1 ? 1 : query.Pagina;
        var tamano = query.Tamano is < 1 or > 200 ? 20 : query.Tamano;
        var ahora = _fechaActual.AhoraUtc;

        var (items, total) = await _documentoRepository.ObtenerListaCreditoAsync(
            pagina,
            tamano,
            query.FiltroDinamico,
            query.ClienteId,
            query.SoloVencidas,
            ahora,
            cancellationToken);

        var hoy = ahora.Date;
        var ids = items.Select(d => d.Id).ToList();
        var montosNotasCredito = await _documentoRepository.ObtenerMontoNotasCreditoPorDocumentosAsync(ids, cancellationToken);
        var montosNotasDebito = await _documentoRepository.ObtenerMontoNotasDebitoPorDocumentosAsync(ids, cancellationToken);

        var dtos = items.Select(d =>
        {
            var dto = DocumentoVentaMapper.ToFacturaCreditoResumen(d, hoy);
            var saldoNeto = Math.Max(
                0m,
                d.SaldoPendiente
                    - (montosNotasCredito.TryGetValue(d.Id, out var montoNc) ? montoNc : 0m)
                    + (montosNotasDebito.TryGetValue(d.Id, out var montoNd) ? montoNd : 0m));
            return dto with { SaldoPendiente = saldoNeto };
        }).ToList();

        return PagedResult<FacturaCreditoResumenDto>.Crear(dtos, pagina, tamano, total);
    }
}
