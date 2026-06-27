using ErrorOr;
using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Inventarios;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Inventarios;

public sealed class ObtenerReporteInventarioRequest
{
    public string? Codigo { get; set; }
    public Guid? CategoriaId { get; set; }
}

public sealed class ObtenerReporteInventarioEndpoint(IMediator mediator)
        : Endpoint<ObtenerReporteInventarioRequest, ReporteInventarioResultadoDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/productos/reportes/inventario");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesInventarioVer));
        Summary(s => s.Summary = "Reporte de inventario (existencias valorizadas)");
    }

    public override async Task HandleAsync(ObtenerReporteInventarioRequest req, CancellationToken ct)
    {
        var query = new ObtenerReporteInventarioQuery(req.Codigo, req.CategoriaId);
        var result = await _mediator.Send(query, ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.OkAsync(result.Value, ct);
    }
}
