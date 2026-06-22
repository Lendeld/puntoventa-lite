using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Commands.Roles;

public sealed record ActualizarPermisosRolCommand(
    Guid RolId,
    Guid PaginaId,
    IReadOnlyList<Guid> PermisosIds) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarPermisosRolHandler(IRolRepository rolRepository, IPermisoCache permisoCache) : IRequestHandler<ActualizarPermisosRolCommand, ErrorOr<Success>>
{
    private readonly IRolRepository _rolRepository = rolRepository;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarPermisosRolCommand command, CancellationToken cancellationToken)
    {
        var rol = await _rolRepository.GetByIdAsync(command.RolId, cancellationToken);
        if (rol is null)
        {
            return RolErrors.NoEncontrado;
        }

        await _rolRepository.ActualizarPermisosAsync(
            command.RolId, command.PaginaId, command.PermisosIds, cancellationToken);

        _permisoCache.InvalidarTodos();
        return Result.Success;
    }
}
