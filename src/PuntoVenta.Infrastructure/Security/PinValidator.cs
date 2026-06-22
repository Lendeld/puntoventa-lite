using ErrorOr;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Infrastructure.Security;

/// <summary>
/// Valida el PIN de un usuario comparando el texto plano contra el hash BCrypt almacenado.
/// Sin lockout — ver decisiones de backup-restore-db.md (deuda conocida).
/// </summary>
public sealed class PinValidator(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher) : IPinValidator, IScopedService
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async ValueTask<ErrorOr<Success>> ValidarAsync(
        Guid usuarioId,
        string pin,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        if (usuario.PinHash is null)
        {
            return UsuarioErrors.PinNoConfigurado;
        }

        if (!_passwordHasher.Verify(pin, usuario.PinHash))
        {
            return UsuarioErrors.PinIncorrecto;
        }

        return Result.Success;
    }
}
