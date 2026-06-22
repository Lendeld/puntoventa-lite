using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.CondicionesVenta;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.CondicionesVenta;

public sealed class ActualizarEstadoCondicionVentaEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/condiciones-venta/{id:guid}/estado");
        Tags("CondicionesVenta");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.CondicionesVentaToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar condición de venta";
            s.Description = "Cambia estado activo/inactivo de la condición de venta";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoCondicionVentaCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
