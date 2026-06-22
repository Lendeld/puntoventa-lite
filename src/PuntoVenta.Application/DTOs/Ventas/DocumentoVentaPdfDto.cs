namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record DocumentoVentaPdfDto(
    byte[] Content,
    string FileName,
    string ContentType);
