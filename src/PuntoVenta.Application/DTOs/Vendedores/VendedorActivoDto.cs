namespace PuntoVenta.Application.DTOs.Vendedores;

public sealed record VendedorActivoDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public bool IsPrincipal { get; init; }
}
