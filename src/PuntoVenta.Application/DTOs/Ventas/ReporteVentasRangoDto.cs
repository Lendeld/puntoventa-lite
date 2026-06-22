namespace PuntoVenta.Application.DTOs.Ventas;

// Una fila por línea de producto (vista Detallado). Los montos ya vienen
// colonizados (si aplica) y con signo NC resuelto por el handler.
public sealed record ReporteVentasRangoFilaDto(
    Guid DocumentoId,
    string Consecutivo,
    DateTime FechaFactura,
    string ClienteIdentificacion,
    string ClienteNombre,
    string MedioPago,
    string CondicionVenta,
    string MonedaCodigo,
    decimal TipoCambio,
    int NumeroLinea,
    string ProductoCodigo,
    string ProductoDetalle,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal Subtotal,
    decimal TarifaPorcentaje,
    decimal MontoImpuesto,
    decimal TotalLinea,
    bool EsColonizado,
    bool EsNotaCredito);

// Una fila por documento (vista Resumido). Omite número de línea, producto,
// cantidad, precio unitario y tarifa(%). El resto se suma por documento.
public sealed record ReporteVentasRangoResumenFilaDto(
    Guid DocumentoId,
    string Consecutivo,
    DateTime FechaFactura,
    string ClienteIdentificacion,
    string ClienteNombre,
    string MedioPago,
    string CondicionVenta,
    string MonedaCodigo,
    decimal TipoCambio,
    decimal Descuento,
    decimal Subtotal,
    decimal MontoImpuesto,
    decimal TotalDocumento,
    bool EsColonizado,
    bool EsNotaCredito);

// Resultado único: una de las dos listas poblada según Detallado, más totales
// generales (ya colonizados y con signo NC aplicado).
public sealed record ReporteVentasRangoResultadoDto(
    bool Detallado,
    bool Colonizado,
    IReadOnlyList<ReporteVentasRangoFilaDto> Filas,
    IReadOnlyList<ReporteVentasRangoResumenFilaDto> Resumen,
    decimal TotalSubtotal,
    decimal TotalDescuento,
    decimal TotalImpuesto,
    decimal TotalGeneral);

// Shape liviano proyectado directo desde EF (sin materializar entidades). Una
// fila por línea de documento; los medios de pago vienen como subcolección.
public sealed record VentaRangoProyeccionDto
{
    public Guid DocumentoId { get; init; }
    public string Consecutivo { get; init; } = string.Empty;
    public DateTime FechaDocumento { get; init; }
    public TipoDocumentoVentaProyeccion TipoDocumento { get; init; }
    public string MonedaCodigo { get; init; } = "CRC";
    public decimal TipoCambio { get; init; } = 1m;
    public string CondicionVentaDetalle { get; init; } = string.Empty;
    public string ClienteIdentificacion { get; init; } = string.Empty;
    public string ClienteNombre { get; init; } = string.Empty;
    public int NumeroLinea { get; init; }
    public string ProductoCodigo { get; init; } = string.Empty;
    public string ProductoDetalle { get; init; } = string.Empty;
    public decimal Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal MontoDescuento { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TarifaPorcentaje { get; init; }
    public decimal MontoImpuesto { get; init; }
    public decimal TotalLinea { get; init; }
    public IReadOnlyList<string> MediosPago { get; init; } = [];
}

// Espejo neutral (Application no referencia el enum de Domain en el DTO público,
// pero sí se proyecta desde él). Solo importan los tipos que entran al reporte.
public enum TipoDocumentoVentaProyeccion
{
    Factura = 1,
    NotaCredito = 3,
    NotaDebito = 4
}
