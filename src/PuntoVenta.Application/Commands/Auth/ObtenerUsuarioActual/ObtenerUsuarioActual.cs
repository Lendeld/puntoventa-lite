using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.ObtenerUsuarioActual;

public sealed record ObtenerUsuarioActualQuery : IRequest<ErrorOr<UsuarioActualDto>>;

public sealed class ObtenerUsuarioActualHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository) : IRequestHandler<ObtenerUsuarioActualQuery, ErrorOr<UsuarioActualDto>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public async ValueTask<ErrorOr<UsuarioActualDto>> Handle(ObtenerUsuarioActualQuery query, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(_usuarioActual.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        return new UsuarioActualDto
        {
            Usuario = usuario.NombreUsuario,
            Nombre = usuario.Nombre,
            Identificacion = usuario.Identificacion,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            TienePin = usuario.TienePin,
            DebeCambiarPassword = usuario.DebeCambiarPassword,
        };
    }
}
