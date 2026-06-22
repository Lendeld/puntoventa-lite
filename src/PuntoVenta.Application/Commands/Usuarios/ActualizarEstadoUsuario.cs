using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Usuarios;

public sealed record ActualizarEstadoUsuarioCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoUsuarioHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository,
    IPermisoCache permisoCache) : IRequestHandler<ActualizarEstadoUsuarioCommand, ErrorOr<bool>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.Id, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        // El propietario solo se gestiona a sí mismo; nadie más lo activa/desactiva.
        if (usuario.EsPropietario && usuario.Id != _usuarioActual.UsuarioId)
        {
            return UsuarioErrors.PropietarioSoloSeEditaASiMismo;
        }

        // El propietario no puede desactivar su propia cuenta (evita autobloqueo).
        if (usuario.EsPropietario && usuario.Activo)
        {
            return UsuarioErrors.PropietarioNoSePuedeDesactivar;
        }

        if (usuario.Activo)
        {
            usuario.Desactivar();
        }
        else
        {
            usuario.Activar();
        }

        await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
        _permisoCache.Invalidar(command.Id);

        return usuario.Activo;
    }
}
