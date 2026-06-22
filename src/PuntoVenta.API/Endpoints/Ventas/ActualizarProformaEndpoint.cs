using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed record ActualizarProformaRequest(
    Guid? ClienteId,
    Guid? VendedorId,
    string CondicionVentaCodigo,
    IReadOnlyList<DocumentoVentaLineaCommand> Lineas,
    int? PlazoCreditoDias = null,
    DateTime? FechaDocumento = null,
    string MonedaCodigo = "CRC",
    decimal TipoCambio = 1m,
    string? Observaciones = null);

public sealed class ActualizarProformaEndpoint(IMediator mediator) : Endpoint<ActualizarProformaRequest, Guid>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/ventas/proformas/{id:guid}");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s => s.Summary = "Actualizar proforma");
    }

    public override async Task HandleAsync(ActualizarProformaRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(
            new ActualizarProformaCommand(
                id,
                req.ClienteId,
                req.VendedorId,
                req.CondicionVentaCodigo,
                req.Lineas,
                req.PlazoCreditoDias,
                req.FechaDocumento,
                req.MonedaCodigo,
                req.TipoCambio,
                req.Observaciones),
            ct);

        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
