using System.Text.Json;

namespace PuntoVenta.API.Middleware;

public sealed class PasswordChangeRequiredMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/auth/cambiar-password",
        "/auth/logout",
        "/auth/validar-token",
        "/auth/usuario-actual"
    };

    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        var requiereCambioPassword = user.Identity?.IsAuthenticated == true &&
            bool.TryParse(user.FindFirst("pwd_change_required")?.Value, out var value) &&
            value;

        if (!requiereCambioPassword)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (AllowedPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = StatusCodes.Status403Forbidden,
            type = "Forbidden",
            title = "Acceso denegado",
            errors = new Dictionary<string, string>
            {
                ["Auth_RequiereCambioPassword"] = "Debe cambiar su contraseña antes de continuar."
            }
        }));
    }
}
