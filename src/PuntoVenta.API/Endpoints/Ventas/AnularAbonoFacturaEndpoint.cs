using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed record AnularAbonoFacturaRequest(string Motivo);

public sealed class AnularAbonoFacturaEndpoint(IMediator mediator) : Endpoint<AnularAbonoFacturaRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/{id:guid}/abonos/{pagoId:guid}/anular");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasFacturasAbonoAnular));
        Summary(s => s.Summary = "Anular un abono de factura a crédito");
    }

    public override async Task HandleAsync(AnularAbonoFacturaRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var pagoId = Route<Guid>("pagoId");
        var result = await _mediator.Send(new AnularAbonoFacturaCommand(id, pagoId, req.Motivo), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
