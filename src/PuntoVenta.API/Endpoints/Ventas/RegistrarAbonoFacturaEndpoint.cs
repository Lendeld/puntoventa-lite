using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class RegistrarAbonoFacturaEndpoint(IMediator mediator) : Endpoint<RegistrarAbonoFacturaRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/facturas/{id:guid}/abonos");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasFacturasAbonar));
        Summary(s => s.Summary = "Registrar abono sobre factura a crédito");
    }

    public override async Task HandleAsync(RegistrarAbonoFacturaRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new RegistrarAbonoFacturaCommand(id, req.Pago, req.FechaPago), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}

public sealed record RegistrarAbonoFacturaRequest(DocumentoVentaPagoCommand Pago, DateTime? FechaPago = null);
