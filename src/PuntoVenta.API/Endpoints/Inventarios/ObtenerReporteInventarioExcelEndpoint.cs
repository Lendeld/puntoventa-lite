using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Inventarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Inventarios;

public sealed class ObtenerReporteInventarioExcelEndpoint(IMediator mediator)
        : Endpoint<ObtenerReporteInventarioRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/productos/reportes/inventario/excel");
        Tags("Productos");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesInventarioVer));
        Summary(s => s.Summary = "Descargar Excel del reporte de inventario");
    }

    public override async Task HandleAsync(ObtenerReporteInventarioRequest req, CancellationToken ct)
    {
        var query = new ObtenerReporteInventarioExcelQuery(req.Codigo, req.CategoriaId, req.ProveedorId);
        var result = await _mediator.Send(query, ct);
        if (result.IsError)
        {
            await Send.ResultAsync(result.Errors.ToProblem());
            return;
        }

        HttpContext.Response.ContentType = result.Value.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{result.Value.FileName}\"";
        HttpContext.Response.Headers.CacheControl = "no-store";
        await HttpContext.Response.Body.WriteAsync(result.Value.Content, ct);
    }
}
