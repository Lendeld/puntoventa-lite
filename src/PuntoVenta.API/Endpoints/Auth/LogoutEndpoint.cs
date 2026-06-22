using System.IdentityModel.Tokens.Jwt;
using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class LogoutRequest
{
    public string? RefreshToken { get; init; }
}

public sealed class LogoutEndpoint(IMediator mediator) : Endpoint<LogoutRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/auth/logout");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Cerrar sesión";
            summary.Description = "Invalida el JWT actual y revoca la sesión de refresh";
        });
    }

    public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (jti is null || expClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var expiracion = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
        var result = await _mediator.Send(
            new LogoutCommand(jti, expiracion, usuarioId, req.RefreshToken),
            ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
