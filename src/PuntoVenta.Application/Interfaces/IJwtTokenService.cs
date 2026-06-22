namespace PuntoVenta.Application.Interfaces;

public sealed record TokenInfo(string Token, string Jti, DateTime Expiracion);

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiracionUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiracionUtc);

public interface IJwtTokenService
{
    TokenInfo GenerarToken(
        Guid usuarioId,
        string nombreUsuario,
        bool requiereCambioPassword = false);
}
