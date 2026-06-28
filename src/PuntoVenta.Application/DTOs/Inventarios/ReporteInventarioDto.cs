namespace PuntoVenta.Application.DTOs.Inventarios;

// Shape liviano proyectado desde EF (sin materializar entidades). Una fila por
// producto del universo (Bien activo que maneja existencia).
public sealed record InventarioReporteProyeccionDto
{
    public Guid ProductoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public string? Categoria { get; init; }   // null si sin categoría
    public string? Proveedor { get; init; }  // null si sin proveedor
    public DateTime FechaCreacion { get; init; }
    public decimal Existencia { get; init; }
    public decimal PrecioUnitario { get; init; }   // precio neto (sin IVA), ver Decisiones
    public decimal? PrecioCosto { get; init; }
    public decimal TarifaPorcentaje { get; init; } // 0 si sin tarifa
}

// Una fila por producto en el reporte (14 columnas del contrato).
public sealed record ReporteInventarioFilaDto(
    Guid ProductoId,
    string Codigo,
    string Nombre,
    string Descripcion,
    string Categoria,
    string Proveedor,
    DateTime FechaCreacion,
    decimal Existencia,
    decimal PrecioCosto,
    decimal PrecioNeto,
    decimal TarifaPorcentaje,
    decimal MontoImpuesto,
    decimal PrecioVenta,
    decimal ValorCosto,
    decimal ValorVenta);

// Resultado del endpoint de datos: filas + 4 totales valorizados.
public sealed record ReporteInventarioResultadoDto(
    IReadOnlyList<ReporteInventarioFilaDto> Filas,
    decimal TotalExistencia,
    decimal TotalValorCosto,
    decimal TotalValorImpuesto,
    decimal TotalValorVenta);
