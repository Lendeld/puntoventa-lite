namespace PuntoVenta.Application.DTOs.Paginas;

public sealed record PaginaMenuDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Ruta { get; init; } = string.Empty;
    public string? Icono { get; init; }
    public int Orden { get; init; }
    public Guid? PaginaPadreId { get; init; }
}
