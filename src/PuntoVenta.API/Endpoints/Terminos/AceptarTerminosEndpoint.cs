using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;

namespace PuntoVenta.API.Endpoints.Terminos;

public sealed class AceptarTerminosEndpoint(IMediator mediator)
    : Endpoint<AceptarTerminosCommand>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/terminos/aceptar");
        Tags("Terminos");
        Summary(s =>
        {
            s.Summary = "Aceptar términos";
            s.Description = "Registra la aceptación de la versión vigente de los términos.";
        });
    }

    public override async Task HandleAsync(AceptarTerminosCommand req, CancellationToken ct)
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
