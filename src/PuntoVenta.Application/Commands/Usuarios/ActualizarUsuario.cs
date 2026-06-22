using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Usuarios;

public sealed record ActualizarUsuarioCommand(
    Guid Id,
    bool Activo,
    Guid? RolId = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarUsuarioHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository,
    IRolRepository rolRepository,
    IPermisoCache permisoCache) : IRequestHandler<ActualizarUsuarioCommand, ErrorOr<Success>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IRolRepository _rolRepository = rolRepository;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.Id, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        // El propietario solo se edita a sí mismo; nadie más lo toca.
        if (usuario.EsPropietario && usuario.Id != _usuarioActual.UsuarioId)
        {
            return UsuarioErrors.PropietarioSoloSeEditaASiMismo;
        }

        if (command.RolId.HasValue)
        {
            // El rol del propietario es inmutable, incluso para él mismo.
            if (usuario.EsPropietario && command.RolId.Value != usuario.RolId)
            {
                return UsuarioErrors.PropietarioRolNoSePuedeCambiar;
            }

            var rol = await _rolRepository.GetByIdAsync(command.RolId.Value, cancellationToken);
            if (rol is null)
            {
                return RolErrors.NoEncontrado;
            }
            usuario.AsignarRol(rol.Id);
        }

        // El propietario no puede desactivar su propia cuenta (evita autobloqueo).
        if (usuario.EsPropietario && !command.Activo)
        {
            return UsuarioErrors.PropietarioNoSePuedeDesactivar;
        }

        if (command.Activo)
        {
            usuario.Activar();
        }
        else
        {
            usuario.Desactivar();
        }

        await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
        _permisoCache.Invalidar(usuario.Id);

        return Result.Success;
    }
}
