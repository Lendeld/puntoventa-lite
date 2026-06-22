using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Queries.Diagnostico.ObtenerAcerca;

namespace PuntoVenta.API.Endpoints.Diagnostico;

public sealed class ObtenerAcercaEndpoint(IMediator mediator) : EndpointWithoutRequest<AcercaDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/acerca");
        Tags("Diagnostico");
        Summary(s =>
        {
            s.Summary = "Acerca de";
            s.Description = "Versión, commit, última migración EF, modo de despliegue y ambiente.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerAcercaQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 200, ct);
    }
}
