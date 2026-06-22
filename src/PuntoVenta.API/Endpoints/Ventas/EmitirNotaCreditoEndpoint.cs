using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class EmitirNotaCreditoEndpoint(IMediator mediator) : Endpoint<EmitirNotaCreditoCommand, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/notas-credito");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasNotasCreditoCrear));
        Summary(s => s.Summary = "Emitir nota de crédito");
    }

    public override async Task HandleAsync(EmitirNotaCreditoCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value, 201, ct);
    }
}
