using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.DTOs.Inventario;

public sealed record MovimientoStockDto(
    Guid Id,
    Guid ProductoId,
    string NombreProducto,
    DateTime FechaUtc,
    TipoDocumentoVenta? TipoDocumentoOrigen,
    Guid? DocumentoVentaId,
    string? ConsecutivoDocumento,
    decimal Delta,
    decimal SaldoResultante,
    Guid? UsuarioId,
    string? Razon);
