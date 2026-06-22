namespace PuntoVenta.Application.DTOs.Vendedores;

public sealed record VendedorDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public bool IsPrincipal { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
