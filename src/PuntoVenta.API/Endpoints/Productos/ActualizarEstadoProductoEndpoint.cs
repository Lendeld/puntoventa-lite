using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Productos;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Productos;

public sealed class ActualizarEstadoProductoEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/productos/{id:guid}/estado");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ProductosToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar producto";
            s.Description = "Cambia el estado activo/inactivo del producto.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoProductoCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
