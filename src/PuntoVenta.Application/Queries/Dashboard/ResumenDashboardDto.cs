namespace PuntoVenta.Application.Queries.Dashboard;

public sealed record ResumenDashboardDto(
    VentasPeriodoDto Hoy,
    VentasMesDto Mes,
    IReadOnlyList<PuntoTendenciaDto> Tendencia,
    IReadOnlyList<MetodoPagoDto> MetodosPago,
    IReadOnlyList<TopProductoDto> TopProductos,
    CuentasPorCobrarDto Cobros);

public sealed record VentasPeriodoDto(decimal Total, int Cantidad);

public sealed record VentasMesDto(
    decimal Total,
    int Cantidad,
    decimal TotalMesAnterior,
    decimal? PorcentajeCambio);

public sealed record PuntoTendenciaDto(DateOnly Fecha, decimal Total);

public sealed record MetodoPagoDto(string Codigo, string Detalle, decimal Total);

public sealed record TopProductoDto(string Nombre, decimal Cantidad, decimal Total);

public sealed record CuentasPorCobrarDto(decimal TotalVencido, int CantidadVencidas);
