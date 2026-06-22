using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.TarifasIvaImpuesto;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.TarifasIvaImpuesto;

public sealed record ObtenerListaTarifaIvaImpuestoQuery(bool? Activo = null) : IRequest<ErrorOr<IReadOnlyList<TarifaIvaImpuestoDto>>>;

public sealed class ObtenerListaTarifaIvaImpuestoHandler(ITarifaIvaImpuestoRepository repository) : IRequestHandler<ObtenerListaTarifaIvaImpuestoQuery, ErrorOr<IReadOnlyList<TarifaIvaImpuestoDto>>>
{
    private readonly ITarifaIvaImpuestoRepository _repository = repository;

    public async ValueTask<ErrorOr<IReadOnlyList<TarifaIvaImpuestoDto>>> Handle(ObtenerListaTarifaIvaImpuestoQuery query, CancellationToken cancellationToken)
    {
        var items = await _repository.ObtenerListaAsync(query.Activo, cancellationToken);
        return items.Select(TarifaIvaImpuestoMapper.ToDto).ToList();
    }
}
