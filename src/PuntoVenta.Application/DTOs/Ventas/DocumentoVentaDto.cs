using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record DocumentoVentaLineaDto
{
    public Guid Id { get; init; }
    public Guid? ProductoId { get; init; }
    public TipoItem TipoItem { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string UnidadMedidaCodigo { get; init; } = string.Empty;
    public decimal Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal MontoDescuento { get; init; }
    public decimal Subtotal { get; init; }
    public decimal MontoImpuesto { get; init; }
    public decimal TotalLinea { get; init; }
    public bool DevuelveInventario { get; init; }
    public bool NoAplicaExistencias { get; init; }
    public bool PermiteModificarPrecioUnitario { get; init; }

    // Acumulado de NCs emitidas que afectan al mismo producto en este documento.
    // Cantidad solo cuenta NCs que devolvieron inventario; subtotal cuenta todas.
    public decimal CantidadDevueltaEnNotasCredito { get; init; }
    public decimal SubtotalAcumuladoNotasCredito { get; init; }
}

public sealed record DocumentoVentaPagoDto
{
    public Guid Id { get; init; }
    public int NumeroAbono { get; init; }
    public string MonedaCodigo { get; init; } = string.Empty;
    public decimal TipoCambioAplicado { get; init; }
    public string MedioPagoCodigo { get; init; } = string.Empty;
    public string MedioPagoDetalleSnapshot { get; init; } = string.Empty;
    public decimal MontoEntregado { get; init; }
    public decimal MontoAplicadoMonedaPago { get; init; }
    public decimal MontoAplicadoDocumento { get; init; }
    public decimal MontoVueltoMonedaPago { get; init; }
    public decimal MontoVueltoDocumento { get; init; }
    public DateTime FechaPago { get; init; }
    public DateTime FechaRegistroUtc { get; init; }
    public Guid? UsuarioRegistroId { get; init; }
    public string? UsuarioRegistroNombre { get; init; }
    public string? Referencia { get; init; }
    public string? Observacion { get; init; }
    public bool Anulado { get; init; }
    public DateTime? FechaAnulacionUtc { get; init; }
    public Guid? UsuarioAnulaId { get; init; }
    public string? UsuarioAnulaNombre { get; init; }
    public string? MotivoAnulacion { get; init; }
}

public sealed record DocumentoVentaReferenciaDto
{
    public Guid Id { get; init; }
    public Guid DocumentoReferenciaId { get; init; }
    public string TipoDocReferencia { get; init; } = string.Empty;
    public DateTime FechaDocumentoReferencia { get; init; }
    public string Razon { get; init; } = string.Empty;
}

public sealed record DocumentoVentaRelacionadoDto
{
    public Guid Id { get; init; }
    public TipoDocumentoVenta TipoDocumento { get; init; }
    public EstadoDocumentoVenta Estado { get; init; }
    public string? Consecutivo { get; init; }
    public DateTime FechaDocumento { get; init; }
    public string TipoDocumentoDetalle { get; init; } = string.Empty;
    public string TipoDocumentoColor { get; init; } = string.Empty;
    public string EstadoDetalle { get; init; } = string.Empty;
    public string EstadoColor { get; init; } = string.Empty;
    public decimal TotalComprobante { get; init; }
    public decimal TotalPagado { get; init; }
    public string MonedaCodigo { get; init; } = string.Empty;
    // Σ notas de crédito emitidas contra este documento (relevante cuando es una
    // ND: su saldo vigente = TotalComprobante − MontoNotasCreditoAplicadas). 0
    // para documentos sin NCs aplicadas.
    public decimal MontoNotasCreditoAplicadas { get; init; }
}

public record DocumentoVentaResumenDto
{
    public Guid Id { get; init; }
    public TipoDocumentoVenta TipoDocumento { get; init; }
    public EstadoDocumentoVenta Estado { get; init; }
    public Guid? ClienteId { get; init; }
    public string? ClienteNombre { get; init; }
    public string? ClienteIdentificacion { get; init; }
    public Guid? VendedorId { get; init; }
    public string? VendedorNombre { get; init; }
    public string? Consecutivo { get; init; }
    public DateTime FechaDocumento { get; init; }
    public string TipoDocumentoDetalle { get; init; } = string.Empty;
    public string TipoDocumentoColor { get; init; } = string.Empty;
    public string EstadoDetalle { get; init; } = string.Empty;
    public string EstadoColor { get; init; } = string.Empty;
    public string CondicionVentaCodigo { get; init; } = string.Empty;
    public string CondicionVentaDetalleSnapshot { get; init; } = string.Empty;
    public decimal TotalComprobante { get; init; }
    public decimal TotalPagado { get; init; }
    public decimal SaldoPendiente { get; init; }
    public string MonedaCodigo { get; init; } = "CRC";
    public DateTime? FechaCancelacion { get; init; }
    public bool EsCredito { get; init; }
    public string? CreadoPor { get; init; }
    public decimal MontoNotasCredito { get; init; }
    public decimal MontoNotasDebito { get; init; }
    // Ajuste de redondeo: delta entre el total preciso (5 dec) y el monto a
    // pagar (2 dec). Permite que reportes/ticket reconcilien neto vs cobrado
    // sin perder decimales. Derivado de TotalComprobante (no se almacena).
    public decimal MontoRedondeo { get; init; }
}

public sealed record FacturaCreditoResumenDto(
    Guid Id,
    string? Consecutivo,
    DateTime FechaDocumento,
    DateTime? FechaVencimiento,
    int? PlazoCreditoDias,
    Guid? ClienteId,
    string? ClienteNombre,
    string? ClienteIdentificacion,
    string CondicionVentaCodigo,
    string CondicionVentaDetalleSnapshot,
    decimal TotalComprobante,
    decimal TotalPagado,
    decimal SaldoPendiente,
    int DiasAtraso,
    bool EsVencida);

public sealed record DocumentoVentaDto : DocumentoVentaResumenDto
{
    public int? PlazoCreditoDias { get; init; }
    public DateTime? FechaVencimiento { get; init; }
    public decimal TipoCambio { get; init; }
    public decimal TotalVenta { get; init; }
    public decimal TotalDescuentos { get; init; }
    public decimal TotalImpuesto { get; init; }
    public string? Observaciones { get; init; }
    public IReadOnlyList<DocumentoVentaLineaDto> Lineas { get; init; } = [];
    public IReadOnlyList<DocumentoVentaPagoDto> Pagos { get; init; } = [];
    public IReadOnlyList<DocumentoVentaReferenciaDto> Referencias { get; init; } = [];
    public DocumentoVentaRelacionadoDto? DocumentoOrigen { get; init; }
    public IReadOnlyList<DocumentoVentaRelacionadoDto> DocumentosGenerados { get; init; } = [];
}
