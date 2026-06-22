using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Security;

public sealed class PermisoRequirement(string clave) : IAuthorizationRequirement
{
    public string Clave { get; } = clave;
}

public sealed class PermisoAuthorizationHandler(IPermisoCache permisoCache) : AuthorizationHandler<PermisoRequirement>
{
    private readonly IPermisoCache _permisoCache = permisoCache;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(sub, out var usuarioId))
        {
            context.Fail();
            return;
        }

        var permisos = await _permisoCache.ObtenerPermisosAsync(usuarioId);

        if (permisos.Contains(requirement.Clave))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
