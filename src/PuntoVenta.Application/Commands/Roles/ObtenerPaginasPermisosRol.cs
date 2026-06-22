using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ObtenerPaginasPermisosRolQuery(Guid RolId) : IRequest<ErrorOr<IReadOnlyList<PaginaPermisosRolTabDto>>>;

public sealed class ObtenerPaginasPermisosRolHandler(IRolRepository rolRepository) : IRequestHandler<ObtenerPaginasPermisosRolQuery, ErrorOr<IReadOnlyList<PaginaPermisosRolTabDto>>>
{
    private readonly IRolRepository _rolRepository = rolRepository;

    public async ValueTask<ErrorOr<IReadOnlyList<PaginaPermisosRolTabDto>>> Handle(ObtenerPaginasPermisosRolQuery query, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(query.RolId, cancellationToken);
        if (rol is null)
        {
            return RolErrors.NoEncontrado;
        }

        var paginas = await _rolRepository.ObtenerPaginasConPermisosAsync(cancellationToken);
        return paginas.ToList();
    }
}
