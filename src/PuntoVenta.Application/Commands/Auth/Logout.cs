using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Application.Commands.Auth;

public sealed record LogoutCommand(string Jti, DateTime ExpiracionUtc, Guid UsuarioId, string? RefreshToken) : IRequest<ErrorOr<Success>>;

public sealed class LogoutHandler(
    ITokenRevocadoRepository tokenRevocadoRepository,
    IRefreshTokenSessionRepository refreshTokenSessionRepository,
    IOpaqueTokenService opaqueTokenService) : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly ITokenRevocadoRepository _tokenRevocadoRepository = tokenRevocadoRepository;
    private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
    private readonly IOpaqueTokenService _opaqueTokenService = opaqueTokenService;

    public async ValueTask<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (!await _tokenRevocadoRepository.EstaRevocadoAsync(command.Jti, cancellationToken))
        {
            var tokenRevocado = TokenRevocado.Crear(command.Jti, command.ExpiracionUtc);
            await _tokenRevocadoRepository.RevocarAsync(tokenRevocado, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var hash = _opaqueTokenService.CalcularHash(command.RefreshToken);
            var session = await _refreshTokenSessionRepository.ObtenerPorTokenHashAsync(hash, cancellationToken);
            if (session is not null)
            {
                session.Revocar(DateTime.UtcNow);
                await _refreshTokenSessionRepository.ActualizarAsync(session, cancellationToken);
                return Result.Success;
            }
        }

        await _refreshTokenSessionRepository.RevocarSesionesUsuarioAsync(command.UsuarioId, DateTime.UtcNow, cancellationToken);
        return Result.Success;
    }
}
