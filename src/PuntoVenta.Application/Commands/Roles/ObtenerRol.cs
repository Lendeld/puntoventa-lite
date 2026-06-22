using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ObtenerRolQuery(Guid Id) : IRequest<ErrorOr<RolDto>>;

public sealed class ObtenerRolHandler(IRolRepository rolRepository) : IRequestHandler<ObtenerRolQuery, ErrorOr<RolDto>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<RolDto>> Handle(ObtenerRolQuery query, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(query.Id, cancellationToken);

        if (rol is null)
        {
            return RolErrors.NoEncontrado;
        }

        return RolMapper.ToDto(rol);
    }
}
