using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class AbonarApartadoEndpoint(IMediator mediator) : Endpoint<AbonarApartadoRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/apartados/{id:guid}/abonos");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.VentasApartadosAbonar));
        Summary(s => s.Summary = "Registrar abono de apartado");
    }

    public override async Task HandleAsync(AbonarApartadoRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new AbonarApartadoCommand(id, req.Pago, req.FechaPago), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}

public sealed record AbonarApartadoRequest(DocumentoVentaPagoCommand Pago, DateTime? FechaPago = null);
