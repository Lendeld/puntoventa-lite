using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Auth.EstablecerPinUsuarioActual;

namespace PuntoVenta.API.Endpoints.Auth;

public sealed class EstablecerPinUsuarioActualEndpoint(IMediator mediator) : Endpoint<EstablecerPinUsuarioActualCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/auth/pin");
        Tags("Auth");
        Summary(summary =>
        {
            summary.Summary = "Establecer o cambiar PIN de seguridad";
            summary.Description = "Permite al usuario autenticado establecer o cambiar su PIN de 6 dígitos. Requiere contraseña actual.";
        });
    }

    public override async Task HandleAsync(EstablecerPinUsuarioActualCommand req, CancellationToken ct)
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
