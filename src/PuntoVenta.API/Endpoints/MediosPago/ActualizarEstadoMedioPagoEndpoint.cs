using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.MediosPago;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.MediosPago;

public sealed class ActualizarEstadoMedioPagoEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/medios-pago/{id:guid}/estado");
        Tags("MediosPago");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.MediosPagoToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar medio de pago";
            s.Description = "Cambia estado activo/inactivo del medio de pago";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoMedioPagoCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
