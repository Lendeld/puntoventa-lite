using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record TicketDataDto(
    string Encabezado,
    string? Direccion,
    string? IdentificacionFiscal,
    string? Telefono,
    string? Correo,
    string? LogoUrl,
    bool MostrarLogo,
    string TipoDocumento,
    string Consecutivo,
    DateTime FechaUtc,
    string? CajaCodigo,
    string? CajaNombre,
    string? VendedorNombre,
    string CondicionVentaDetalle,
    string ClienteNombre,
    string? ClienteIdentificacion,
    IReadOnlyList<TicketLineaDto> Lineas,
    IReadOnlyList<TicketPagoDto> Pagos,
    decimal Subtotal,
    decimal Descuentos,
    decimal Impuestos,
    decimal Total,
    decimal Pagado,
    decimal Saldo,
    string MonedaCodigo,
    decimal TipoCambio,
    string? MensajePie,
    string? Observaciones,
    bool AplicaCopiaClienteNegocio,
    bool MostrarCodigoBarras,
    IReadOnlyList<TicketLineaPieDto> LineasPie,
    string? ReferenciaTipoDocumento = null,
    string? ReferenciaConsecutivo = null,
    string? ReferenciaRazon = null,
    // Encabezado del ticket térmico ya resuelto (texto + negrita) en el orden
    // configurado. Null/vacío = el agente usa el encabezado fijo histórico.
    IReadOnlyList<TicketEncabezadoLineaDto>? LineasEncabezado = null,
    bool EsRecibo = false,
    decimal SaldoAnterior = 0m,
    decimal SaldoNuevo = 0m,
    bool EsReciboAnulado = false,
    DateTime? FechaAnulacionUtc = null,
    string? UsuarioAnulaNombre = null,
    string? MotivoAnulacion = null);

public sealed record TicketEncabezadoLineaDto(string Texto, bool Negrita);

public sealed record TicketLineaDto(
    string Codigo,
    string Descripcion,
    decimal Cantidad,
    string UnidadMedidaCodigo,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal PorcentajeImpuesto,
    decimal Total);

public sealed record TicketLineaPieDto(
    string Texto,
    AlineacionLineaPie Alineacion,
    bool Negrita);

public sealed record TicketPagoDto(
    Guid Id,
    DateTime FechaUtc,
    string MedioPagoDetalle,
    string MonedaCodigo,
    decimal MontoAplicado,
    decimal MontoEntregado,
    decimal MontoVuelto,
    string? Referencia,
    int NumeroAbono = 0,
    DateTime? FechaRegistroUtc = null,
    bool Anulado = false,
    DateTime? FechaAnulacionUtc = null,
    string? UsuarioAnulaNombre = null,
    string? MotivoAnulacion = null);
