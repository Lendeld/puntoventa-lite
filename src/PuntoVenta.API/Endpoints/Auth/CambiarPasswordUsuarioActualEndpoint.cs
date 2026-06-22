using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth.CambiarPasswordUsuarioActual;
using PuntoVenta.Application.DTOs.Auth;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class CambiarPasswordUsuarioActualEndpoint(IMediator mediator) : Endpoint<CambiarPasswordUsuarioActualCommand, AuthFlowResponse>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/auth/cambiar-password");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Cambiar contraseña usuario actual";
            summary.Description = "Permite a usuario autenticado cambiar su contraseña";
        });
    }

    public override async Task HandleAsync(CambiarPasswordUsuarioActualCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
