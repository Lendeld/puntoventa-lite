using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.TarifasIvaImpuesto;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.TarifasIvaImpuesto;

public sealed class ActualizarEstadoTarifaIvaImpuestoEndpoint(IMediator mediator) : EndpointWithoutRequest<bool>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/tarifas-iva/{id:guid}/estado");
        Tags("TarifasIvaImpuesto");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.TarifasIvaToggle));
        Summary(s =>
        {
            s.Summary = "Activar / desactivar tarifa IVA";
            s.Description = "Cambia estado activo/inactivo de la tarifa IVA";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new ActualizarEstadoTarifaIvaImpuestoCommand(id), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
