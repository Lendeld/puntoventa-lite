using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.ActualizarUsuarioActual;

public sealed record ActualizarUsuarioActualCommand(
    string Nombre,
    string Identificacion,
    string? Correo = null,
    string? Telefono = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarUsuarioActualHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository) : IRequestHandler<ActualizarUsuarioActualCommand, ErrorOr<Success>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarUsuarioActualCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(_usuarioActual.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        var resultado = usuario.ActualizarPerfil(
            command.Nombre,
            command.Identificacion,
            command.Correo,
            command.Telefono);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
        return Result.Success;
    }
}
