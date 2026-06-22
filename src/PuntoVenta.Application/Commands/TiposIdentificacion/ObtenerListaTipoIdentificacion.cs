using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.TiposIdentificacion;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.TiposIdentificacion;

public sealed record ObtenerListaTipoIdentificacionQuery(bool? Activo = null) : IRequest<ErrorOr<IReadOnlyList<TipoIdentificacionDto>>>;

public sealed class ObtenerListaTipoIdentificacionHandler(ITipoIdentificacionRepository tipoIdentificacionRepository) : IRequestHandler<ObtenerListaTipoIdentificacionQuery, ErrorOr<IReadOnlyList<TipoIdentificacionDto>>>
{
    private readonly ITipoIdentificacionRepository _tipoIdentificacionRepository = tipoIdentificacionRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<TipoIdentificacionDto>>> Handle(ObtenerListaTipoIdentificacionQuery query, CancellationToken cancellationToken)
    {
        var items = await _tipoIdentificacionRepository.ObtenerListaAsync(query.Activo, cancellationToken);

        return items.Select(TipoIdentificacionMapper.ToDto).ToList();
    }
}
