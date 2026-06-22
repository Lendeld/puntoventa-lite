using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth;

public sealed record ValidarPermisoQuery(string Clave) : IRequest<ErrorOr<Success>>;

public sealed class ValidarPermisoHandler(IUsuarioActual usuarioActual, IPermisoCache permisoCache) : IRequestHandler<ValidarPermisoQuery, ErrorOr<Success>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IPermisoCache _permisoCache = permisoCache;

    public async ValueTask<ErrorOr<Success>> Handle(ValidarPermisoQuery query, CancellationToken cancellationToken)
    {
        if (_usuarioActual.RequiereCambioPassword)
        {
            return UsuarioErrors.RequiereCambioPassword;
        }

        var permisos = await _permisoCache.ObtenerPermisosAsync(
            _usuarioActual.UsuarioId,
            cancellationToken);

        if (!permisos.Contains(query.Clave))
        {
            return Error.Forbidden();
        }

        return Result.Success;
    }
}
