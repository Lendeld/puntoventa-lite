namespace PuntoVenta.Application.DTOs.Inventarios;

public sealed record ReporteInventarioExcelDto(byte[] Content, string FileName, string ContentType);
