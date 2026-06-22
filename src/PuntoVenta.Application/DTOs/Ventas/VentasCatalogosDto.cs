namespace PuntoVenta.Application.DTOs.Ventas;

public sealed record VentaCatalogoItemDto(
    int Valor,
    string Detalle,
    string Color);

public sealed record VentasCatalogosDto(
    IReadOnlyList<VentaCatalogoItemDto> TiposDocumento,
    IReadOnlyList<VentaCatalogoItemDto> EstadosDocumento);
