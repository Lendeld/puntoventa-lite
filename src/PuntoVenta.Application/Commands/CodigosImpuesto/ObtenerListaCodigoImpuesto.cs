using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.CodigosImpuesto;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.CodigosImpuesto;

public sealed record ObtenerListaCodigoImpuestoQuery(bool? Activo = null) : IRequest<ErrorOr<IReadOnlyList<CodigoImpuestoDto>>>;

public sealed class ObtenerListaCodigoImpuestoHandler(ICodigoImpuestoRepository repository) : IRequestHandler<ObtenerListaCodigoImpuestoQuery, ErrorOr<IReadOnlyList<CodigoImpuestoDto>>>
{
    private readonly ICodigoImpuestoRepository _repository = repository;

    public async ValueTask<ErrorOr<IReadOnlyList<CodigoImpuestoDto>>> Handle(ObtenerListaCodigoImpuestoQuery query, CancellationToken cancellationToken)
    {
        var items = await _repository.ObtenerListaAsync(query.Activo, cancellationToken);
        return items.Select(CodigoImpuestoMapper.ToDto).ToList();
    }
}
