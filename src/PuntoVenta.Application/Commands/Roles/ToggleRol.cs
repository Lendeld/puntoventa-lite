using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ToggleRolCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ToggleRolHandler(IRolRepository rolRepository, IPermisoCache permisoCache) : IRequestHandler<ToggleRolCommand, ErrorOr<bool>>
{
    private readonly IRolRepository _rolRepository = rolRepository;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<bool>> Handle(ToggleRolCommand command, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(command.Id, cancellationToken);
        if (rol is null)
        {
            return RolErrors.NoEncontrado;
        }

        if (rol.IsPrincipal)
        {
            return RolErrors.RolPrincipalNoPermiteCambiarEstado;
        }

        if (rol.Activo)
        {
            rol.Desactivar();
        }
        else
        {
            rol.Activar();
        }

        await _rolRepository.UpdateAsync(rol, cancellationToken);
        _permisoCache.InvalidarTodos();
        return rol.Activo;
    }
}
