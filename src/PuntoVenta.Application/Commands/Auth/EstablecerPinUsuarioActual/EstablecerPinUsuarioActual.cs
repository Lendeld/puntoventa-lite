using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.EstablecerPinUsuarioActual;

public sealed record EstablecerPinUsuarioActualCommand(string PasswordActual, string PinNuevo)
    : IRequest<ErrorOr<Success>>;

public sealed class EstablecerPinUsuarioActualHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<EstablecerPinUsuarioActualCommand, ErrorOr<Success>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async ValueTask<ErrorOr<Success>> Handle(
        EstablecerPinUsuarioActualCommand command,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(_usuarioActual.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        if (!_passwordHasher.Verify(command.PasswordActual, usuario.PasswordHash))
        {
            return UsuarioErrors.PasswordActualIncorrecta;
        }

        var pinHash = _passwordHasher.Hash(command.PinNuevo);
        var resultado = usuario.EstablecerPin(pinHash);
        if (resultado.IsError) return resultado.Errors;

        await _usuarioRepository.UpdateAsync(usuario, cancellationToken);

        return Result.Success;
    }
}
