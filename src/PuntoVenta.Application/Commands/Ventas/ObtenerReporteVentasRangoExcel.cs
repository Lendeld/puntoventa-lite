using System.Globalization;
using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerReporteVentasRangoExcelQuery(
    DateTime FechaDesdeUtc,
    DateTime FechaHastaUtc,
    string? Consecutivo,
    bool Colonizar,
    bool Detallado) : IRequest<ErrorOr<ReporteVentasRangoExcelDto>>;

public sealed class ObtenerReporteVentasRangoExcelHandler(
    IMediator mediator,
    IReporteVentasExcelService excelService)
        : IRequestHandler<ObtenerReporteVentasRangoExcelQuery, ErrorOr<ReporteVentasRangoExcelDto>>
{
    private const string ContentTypeXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    private readonly IMediator _mediator = mediator;
    private readonly IReporteVentasExcelService _excelService = excelService;

    public async ValueTask<ErrorOr<ReporteVentasRangoExcelDto>> Handle(
        ObtenerReporteVentasRangoExcelQuery query,
        CancellationToken cancellationToken)
    {
        // Reutiliza la lógica de datos (colonización/signo/agregación) — no duplica.
        var datos = await _mediator.Send(
            new ObtenerReporteVentasRangoQuery(
                query.FechaDesdeUtc,
                query.FechaHastaUtc,
                query.Consecutivo,
                query.Colonizar,
                query.Detallado),
            cancellationToken);

        if (datos.IsError)
        {
            return datos.Errors;
        }

        var content = _excelService.Generar(datos.Value, query.FechaDesdeUtc, query.FechaHastaUtc);
        var fileName =
            $"reporte-ventas-{FechaArchivo(query.FechaDesdeUtc)}-{FechaArchivo(query.FechaHastaUtc)}.xlsx";

        return new ReporteVentasRangoExcelDto(content, fileName, ContentTypeXlsx);
    }

    private static string FechaArchivo(DateTime utc)
    {
        var kindUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(kindUtc, ZonaCR);
        return local.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }
}
