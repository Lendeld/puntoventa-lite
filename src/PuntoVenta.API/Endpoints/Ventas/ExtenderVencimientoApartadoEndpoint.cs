using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ExtenderVencimientoApartadoEndpoint(IMediator mediator) : Endpoint<ExtenderVencimientoApartadoRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/apartados/{id:guid}/extender-vencimiento");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasApartadosExtender));
        Summary(s => s.Summary = "Extender vencimiento de apartado");
    }

    public override async Task HandleAsync(ExtenderVencimientoApartadoRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ExtenderVencimientoApartadoCommand(id, req.FechaVencimiento), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}

public sealed record ExtenderVencimientoApartadoRequest(DateTime FechaVencimiento);
