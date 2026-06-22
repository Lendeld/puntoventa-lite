using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Queries.Dashboard.ObtenerResumenDashboard;

public sealed record ObtenerResumenDashboardQuery() : IRequest<ErrorOr<ResumenDashboardDto>>;

public sealed class ObtenerResumenDashboardHandler
    : IRequestHandler<ObtenerResumenDashboardQuery, ErrorOr<ResumenDashboardDto>>
{
    private const int TopProductos = 5;
    private const int DiasTendencia = 30;
    private static readonly TimeZoneInfo ZonaCR = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    private readonly IDocumentoVentaRepository _documentoRepository;
    private readonly IFechaActual _fechaActual;

    public ObtenerResumenDashboardHandler(
        IDocumentoVentaRepository documentoRepository,
        IFechaActual fechaActual)
    {
        _documentoRepository = documentoRepository;
        _fechaActual = fechaActual;
    }

    public async ValueTask<ErrorOr<ResumenDashboardDto>> Handle(
        ObtenerResumenDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var hoy = _fechaActual.Hoy;
        var manana = hoy.AddDays(1);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var inicioTendencia = hoy.AddDays(-(DiasTendencia - 1));

        var mesAnterior = inicioMes.AddMonths(-1);
        var diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
        var diaCorte = Math.Min(hoy.Day, diasMesAnterior);
        var finMesAnterior = new DateOnly(mesAnterior.Year, mesAnterior.Month, diaCorte).AddDays(1);

        var ventasHoy = await _documentoRepository.ObtenerResumenVentasAsync(
            AUtc(hoy), AUtc(manana), cancellationToken);

        var ventasMes = await _documentoRepository.ObtenerResumenVentasAsync(
            AUtc(inicioMes), AUtc(manana), cancellationToken);

        var ventasMesAnterior = await _documentoRepository.ObtenerResumenVentasAsync(
            AUtc(mesAnterior), AUtc(finMesAnterior), cancellationToken);

        var tendencia = await _documentoRepository.ObtenerTendenciaVentasAsync(
            AUtc(inicioTendencia), AUtc(manana), cancellationToken);

        var metodosPago = await _documentoRepository.ObtenerVentasPorMetodoPagoAsync(
            AUtc(inicioMes), AUtc(manana), cancellationToken);

        var topProductos = await _documentoRepository.ObtenerTopProductosAsync(
            AUtc(inicioMes), AUtc(manana), TopProductos, cancellationToken);

        var cobros = await _documentoRepository.ObtenerCuentasPorCobrarVencidasAsync(
            AUtc(hoy), cancellationToken);

        var mes = new VentasMesDto(
            ventasMes.Total,
            ventasMes.Cantidad,
            ventasMesAnterior.Total,
            ventasMesAnterior.Total == 0m
                ? null
                : Math.Round((ventasMes.Total - ventasMesAnterior.Total) / ventasMesAnterior.Total * 100m, 1));

        var resultado = new ResumenDashboardDto(
            ventasHoy,
            mes,
            tendencia,
            metodosPago,
            topProductos,
            cobros);

        return resultado;
    }

    private static DateTime AUtc(DateOnly fecha)
        => TimeZoneInfo.ConvertTimeToUtc(
            fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
            ZonaCR);
}
