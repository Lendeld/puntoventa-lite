using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.CondicionesVenta;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.CondicionesVenta;

public sealed record ObtenerListaCondicionVentaQuery(bool? Activo = null) : IRequest<ErrorOr<IReadOnlyList<CondicionVentaDto>>>;

public sealed class ObtenerListaCondicionVentaHandler(ICondicionVentaRepository repository) : IRequestHandler<ObtenerListaCondicionVentaQuery, ErrorOr<IReadOnlyList<CondicionVentaDto>>>
{
    private readonly ICondicionVentaRepository _repository = repository;

    public async ValueTask<ErrorOr<IReadOnlyList<CondicionVentaDto>>> Handle(ObtenerListaCondicionVentaQuery query, CancellationToken cancellationToken)
    {
        var items = await _repository.ObtenerListaAsync(query.Activo, cancellationToken);
        return items.Select(CondicionVentaMapper.ToDto).ToList();
    }
}
