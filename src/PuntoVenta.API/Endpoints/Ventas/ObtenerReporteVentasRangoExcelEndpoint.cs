using FastEndpoints;
using Mediator;
using PuntoVenta.API.Extensions;
using PuntoVenta.Application.Commands.Ventas;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Ventas;

public sealed class ObtenerReporteVentasRangoExcelEndpoint(IMediator mediator)
        : Endpoint<ObtenerReporteVentasRangoRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/ventas/reportes/rango/excel");
        Tags("Ventas");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.ReportesVentasRangoVer));
        DontThrowIfValidationFails();
        Summary(s => s.Summary = "Descargar Excel del reporte de ventas por rango");
    }

    public override async Task HandleAsync(ObtenerReporteVentasRangoRequest req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await Send.ResultAsync(ValidationFailures.AErrores().ToProblem());
            return;
        }

        var query = new ObtenerReporteVentasRangoExcelQuery(
            req.FechaDesde.UtcDateTime,
            req.FechaHasta.UtcDateTime,
            req.Consecutivo,
            req.Colonizar,
            req.Detallado);

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
