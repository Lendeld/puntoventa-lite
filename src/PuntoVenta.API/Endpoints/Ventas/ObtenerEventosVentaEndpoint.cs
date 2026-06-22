using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerEventosVentaRequest
{
    public Guid Id { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

public sealed class ObtenerEventosVentaEndpoint(IMediator mediator)
        : Endpoint<ObtenerEventosVentaRequest, DocumentoVentaEventoListaDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/{Id}/eventos");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.FacturacionVer));
        Summary(s =>
        {
            s.Summary = "Historia de eventos del documento de venta";
            s.Description = "Lista paginada de eventos en orden cronológico descendente.";
        });
    }

    public override async Task HandleAsync(ObtenerEventosVentaRequest req, CancellationToken ct)
    {
        var query = new ObtenerEventosVentaQuery(req.Id, req.Skip, req.Take);
        var result = await _mediator.Send(query, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
