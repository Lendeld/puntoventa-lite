using ErrorOr;
using Mediator;
using Microsoft.Extensions.Logging;
using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<ErrorOr<AuthFlowResponse>>;

public sealed class RefreshTokenHandler(
    IRefreshTokenSessionRepository refreshTokenSessionRepository,
    IUsuarioRepository usuarioRepository,
    IJwtTokenService jwtTokenService,
    IOpaqueTokenService opaqueTokenService,
    IAuthSettings authSettings,
    ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthFlowResponse>>
{
    private static readonly TimeSpan ReuseGraceWindow = TimeSpan.FromMinutes(2);

    private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IOpaqueTokenService _opaqueTokenService = opaqueTokenService;
    private readonly IAuthSettings _authSettings = authSettings;
    private readonly ILogger<RefreshTokenHandler> _logger = logger;

    public async ValueTask<ErrorOr<AuthFlowResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var ahoraUtc = DateTime.UtcNow;
        var refreshTokenHash = _opaqueTokenService.CalcularHash(command.RefreshToken);
        var session = await _refreshTokenSessionRepository.ObtenerPorTokenHashAsync(refreshTokenHash, cancellationToken);

        if (session is null)
        {
            return UsuarioErrors.CredencialesInvalidas;
        }

        if (session.FueRotado)
        {
            var replacement = await ObtenerReemplazoValidoAsync(session, ahoraUtc, cancellationToken);

            if (replacement is null)
            {
                await _refreshTokenSessionRepository.RevocarSesionesUsuarioAsync(session.UsuarioId, ahoraUtc, cancellationToken);
                return UsuarioErrors.CredencialesInvalidas;
            }

            var segundosDesdeRotacion = (int)(ahoraUtc - session.RevocadoEnUtc!.Value).TotalSeconds;
            _logger.LogWarning(
                "Reuso de token rotado dentro de la ventana de gracia. UsuarioId={UsuarioId} SegundosDesdeRotacion={SegundosDesdeRotacion} Ip={Ip}",
                session.UsuarioId,
                segundosDesdeRotacion,
                command.IpAddress);

            session = replacement;
        }
        else if (session.RevocadoEnUtc is not null)
        {
            await _refreshTokenSessionRepository.RevocarSesionesUsuarioAsync(session.UsuarioId, ahoraUtc, cancellationToken);
            return UsuarioErrors.CredencialesInvalidas;
        }

        if (!session.EstaActivo(ahoraUtc))
        {
            session.Revocar(ahoraUtc);
            await _refreshTokenSessionRepository.ActualizarAsync(session, cancellationToken);
            return UsuarioErrors.CredencialesInvalidas;
        }

        var usuario = await _usuarioRepository.GetByIdAsync(session.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            await _refreshTokenSessionRepository.RevocarSesionesUsuarioAsync(session.UsuarioId, ahoraUtc, cancellationToken);
            return UsuarioErrors.CredencialesInvalidas;
        }

        var nuevoRefreshToken = _opaqueTokenService.GenerarToken();
        var nuevoRefreshTokenHash = _opaqueTokenService.CalcularHash(nuevoRefreshToken);
        var nuevaExpiracionUtc = ahoraUtc.AddDays(_authSettings.RefreshExpiracionDias);

        session.MarcarUso(ahoraUtc, command.IpAddress);
        session.Revocar(ahoraUtc, nuevoRefreshTokenHash);
        await _refreshTokenSessionRepository.ActualizarAsync(session, cancellationToken);

        var nuevaSesion = RefreshTokenSession.Crear(
            usuario.Id,
            nuevoRefreshTokenHash,
            ahoraUtc,
            nuevaExpiracionUtc,
            command.IpAddress);
        nuevaSesion.MarcarUso(ahoraUtc, command.IpAddress);
        await _refreshTokenSessionRepository.AgregarAsync(nuevaSesion, cancellationToken);

        var accessToken = _jwtTokenService.GenerarToken(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.DebeCambiarPassword);

        return new AuthFlowResponse
        {
            RequiresPasswordChange = usuario.DebeCambiarPassword,
            AccessToken = accessToken.Token,
            AccessTokenExpiracionUtc = accessToken.Expiracion,
            RefreshToken = nuevoRefreshToken,
            RefreshTokenExpiracionUtc = nuevaExpiracionUtc
        };
    }

    private async Task<RefreshTokenSession?> ObtenerReemplazoValidoAsync(
        RefreshTokenSession session,
        DateTime ahoraUtc,
        CancellationToken cancellationToken)
    {
        if (session.RevocadoEnUtc is null ||
            string.IsNullOrWhiteSpace(session.ReemplazadoPorTokenHash) ||
            ahoraUtc - session.RevocadoEnUtc.Value > ReuseGraceWindow)
        {
            return null;
        }

        var replacement = await _refreshTokenSessionRepository.ObtenerReemplazoPorTokenHashAsync(
            session.ReemplazadoPorTokenHash,
            cancellationToken);

        return replacement is not null && replacement.EstaActivo(ahoraUtc) ? replacement : null;
    }
}
