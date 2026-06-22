using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Paginas;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ObtenerPermisosRolPorPaginaQuery(Guid RolId, Guid PaginaId) : IRequest<ErrorOr<PermisosRolPorPaginaDto>>;

public sealed class ObtenerPermisosRolPorPaginaHandler(IRolRepository rolRepository) : IRequestHandler<ObtenerPermisosRolPorPaginaQuery, ErrorOr<PermisosRolPorPaginaDto>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<PermisosRolPorPaginaDto>> Handle(ObtenerPermisosRolPorPaginaQuery query, CancellationToken cancellationToken)
    {
        var rolExiste = await _rolRepository.GetByIdAsync(query.RolId, cancellationToken);
        if (rolExiste is null)
        {
            return RolErrors.NoEncontrado;
        }

        var resultado = await _rolRepository.ObtenerPermisosAgrupadosPorPaginaAsync(
            query.RolId, query.PaginaId, cancellationToken);

        if (resultado is null)
        {
            return PaginaErrors.NoEncontrada;
        }

        return resultado;
    }
}
