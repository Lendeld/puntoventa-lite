using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Negocios;

namespace PuntoVenta.API.Endpoints.Terminos;

public sealed class ObtenerEstadoTerminosEndpoint(IMediator mediator)
    : EndpointWithoutRequest<EstadoTerminosDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/terminos/estado");
        Tags("Terminos");
        Summary(s =>
        {
            s.Summary = "Estado de aceptación de términos";
            s.Description = "Indica si ya se aceptó la versión vigente de los términos.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerEstadoTerminosQuery(), ct);

        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
