using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class UsuarioActual(IHttpContextAccessor httpContextAccessor) : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UsuarioId
    {
        get
        {
            var value = (_httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value) ?? throw new InvalidOperationException("No hay usuario autenticado en el contexto actual.");
            return Guid.Parse(value);
        }
    }

    public string NombreUsuario
    {
        get
        {
            var value = (_httpContextAccessor.HttpContext?.User
                .FindFirst("usuario")?.Value) ?? throw new InvalidOperationException("No hay usuario autenticado en el contexto actual.");
            return value;
        }
    }

    public bool RequiereCambioPassword =>
        bool.TryParse(
            _httpContextAccessor.HttpContext?.User.FindFirst("pwd_change_required")?.Value,
            out var requiereCambioPassword) &&
        requiereCambioPassword;
}
