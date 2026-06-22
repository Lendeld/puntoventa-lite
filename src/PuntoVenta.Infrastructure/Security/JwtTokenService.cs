using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtSettings> settings) : IJwtTokenService, IScopedService
{
    private readonly JwtSettings _settings = settings.Value;

    public TokenInfo GenerarToken(
        Guid usuarioId,
        string nombreUsuario,
        bool requiereCambioPassword = false)
    {
        var jti = Guid.NewGuid().ToString();
        var expiracion = DateTime.UtcNow.AddMinutes(_settings.ExpiracionMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("usuario", nombreUsuario),
            new("pwd_change_required", requiereCambioPassword.ToString().ToLowerInvariant()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: [.. claims],
            expires: expiracion,
            signingCredentials: credentials);

        return new TokenInfo(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Jti: jti,
            Expiracion: expiracion);
    }
}
