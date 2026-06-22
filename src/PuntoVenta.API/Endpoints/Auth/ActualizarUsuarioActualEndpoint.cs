using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth.ActualizarUsuarioActual;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class ActualizarUsuarioActualEndpoint(IMediator mediator) : Endpoint<ActualizarUsuarioActualCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/auth/usuario-actual");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Actualizar usuario actual";
            summary.Description = "Permite a usuario autenticado actualizar su perfil personal.";
        });
    }

    public override async Task HandleAsync(ActualizarUsuarioActualCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
