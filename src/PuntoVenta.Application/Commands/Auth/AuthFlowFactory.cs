using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Tokens;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth;

internal static class AuthFlowFactory
{
    public static async Task<AuthFlowResponse> CrearSesionAsync(
        Usuario usuario,
        bool requiereCambioPassword,
        string? ipAddress,
        DateTime ahoraUtc,
        IAuthSettings authSettings,
        IJwtTokenService jwtTokenService,
        IOpaqueTokenService opaqueTokenService,
        IRefreshTokenSessionRepository refreshTokenSessionRepository,
        CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.GenerarToken(
            usuario.Id,
            usuario.NombreUsuario,
            requiereCambioPassword);
        var refreshToken = opaqueTokenService.GenerarToken();
        var refreshTokenExpiracionUtc = ahoraUtc.AddDays(authSettings.RefreshExpiracionDias);
        var refreshTokenHash = opaqueTokenService.CalcularHash(refreshToken);

        var session = RefreshTokenSession.Crear(
            usuario.Id,
            refreshTokenHash,
            ahoraUtc,
            refreshTokenExpiracionUtc,
            ipAddress);

        await refreshTokenSessionRepository.AgregarAsync(session, cancellationToken);

        return new AuthFlowResponse
        {
            RequiresPasswordChange = requiereCambioPassword,
            AccessToken = accessToken.Token,
            AccessTokenExpiracionUtc = accessToken.Expiracion,
            RefreshToken = refreshToken,
            RefreshTokenExpiracionUtc = refreshTokenExpiracionUtc
        };
    }
}
