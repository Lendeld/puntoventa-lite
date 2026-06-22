using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth;
using PuntoVenta.Application.DTOs.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class RefreshTokenEndpoint(IMediator mediator) : Endpoint<RefreshTokenRequest, AuthFlowResponse>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(
                req.RefreshToken,
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
