using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerCatalogosVentasQuery : IRequest<ErrorOr<VentasCatalogosDto>>;

public sealed class ObtenerCatalogosVentasHandler : IRequestHandler<ObtenerCatalogosVentasQuery, ErrorOr<VentasCatalogosDto>>
{
    public ValueTask<ErrorOr<VentasCatalogosDto>> Handle(ObtenerCatalogosVentasQuery query, CancellationToken cancellationToken)
    {
        var tiposDocumento = Enum.GetValues<TipoDocumentoVenta>()
            .Select(VentasCatalogosMapper.ToCatalogoItem)
            .ToList();

        var estadosDocumento = Enum.GetValues<EstadoDocumentoVenta>()
            .Select(VentasCatalogosMapper.ToCatalogoItem)
            .ToList();

        return ValueTask.FromResult<ErrorOr<VentasCatalogosDto>>(new VentasCatalogosDto(tiposDocumento, estadosDocumento));
    }
}
