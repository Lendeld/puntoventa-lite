namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record ReporteVentasRangoExcelDto(byte[] Content, string FileName, string ContentType);
