using System.Globalization;
using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Inventarios;

public sealed record ObtenerReporteInventarioExcelQuery(
    string? Codigo,
    Guid? CategoriaId,
    Guid? ProveedorId) : IRequest<ErrorOr<ReporteInventarioExcelDto>>;

public sealed class ObtenerReporteInventarioExcelHandler(
    IMediator mediator,
    IReporteInventarioExcelService excelService)
        : IRequestHandler<ObtenerReporteInventarioExcelQuery, ErrorOr<ReporteInventarioExcelDto>>
{
    private const string ContentTypeXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    private readonly IMediator _mediator = mediator;
    private readonly IReporteInventarioExcelService _excelService = excelService;

    public async ValueTask<ErrorOr<ReporteInventarioExcelDto>> Handle(
        ObtenerReporteInventarioExcelQuery query,
        CancellationToken cancellationToken)
    {
        // Reutiliza la lógica de datos — no duplica la proyección ni el cap.
        var datos = await _mediator.Send(
            new ObtenerReporteInventarioQuery(query.Codigo, query.CategoriaId, query.ProveedorId),
            cancellationToken);

        if (datos.IsError)
        {
            return datos.Errors;
        }

        var content = _excelService.Generar(datos.Value);
        var fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaCR)
            .ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var fileName = $"reporte-inventario-{fecha}.xlsx";

        return new ReporteInventarioExcelDto(content, fileName, ContentTypeXlsx);
    }
}
