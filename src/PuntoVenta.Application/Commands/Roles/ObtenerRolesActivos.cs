using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ObtenerRolesActivosQuery : IRequest<ErrorOr<IReadOnlyList<RolDto>>>;

public sealed class ObtenerRolesActivosHandler(IRolRepository rolRepository) : IRequestHandler<ObtenerRolesActivosQuery, ErrorOr<IReadOnlyList<RolDto>>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<RolDto>>> Handle(ObtenerRolesActivosQuery query, CancellationToken cancellationToken)
    {
        var roles = await _rolRepository.ObtenerActivosAsync(cancellationToken);
        return roles.Select(RolMapper.ToDto).ToList();
    }
}
