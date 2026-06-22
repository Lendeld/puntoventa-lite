using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Builder;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth;
using PuntoVenta.Application.DTOs.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class LoginRequest
{
    public string NombreUsuario { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginEndpoint(IMediator mediator) : Endpoint<LoginRequest, AuthFlowResponse>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Iniciar sesión";
            summary.Description = "Autentica un usuario y devuelve tokens o un challenge OTP";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var command = new LoginCommand(
            req.NombreUsuario,
            req.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _mediator.Send(command, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
