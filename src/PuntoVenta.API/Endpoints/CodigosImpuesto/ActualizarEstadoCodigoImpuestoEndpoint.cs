using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.CodigosImpuesto;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.CodigosImpuesto;

public sealed class ActualizarEstadoCodigoImpuestoEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/codigos-impuesto/{id:guid}/estado");
        Tags("CodigosImpuesto");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CodigosImpuestoToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar código de impuesto";
            s.Description = "Cambia estado activo/inactivo del código de impuesto";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoCodigoImpuestoCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
