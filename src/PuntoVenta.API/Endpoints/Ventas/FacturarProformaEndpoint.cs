using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed record FacturarProformaRequest(
    IReadOnlyList<DocumentoVentaPagoCommand> Pagos);

public sealed class FacturarProformaEndpoint(IMediator mediator) : Endpoint<FacturarProformaRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/ventas/proformas/{id:guid}/facturar");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s => s.Summary = "Facturar proforma");
    }

    public override async Task HandleAsync(FacturarProformaRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new FacturarProformaCommand(id, req.Pagos), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
