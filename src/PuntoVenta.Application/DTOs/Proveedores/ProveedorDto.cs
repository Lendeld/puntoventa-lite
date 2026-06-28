namespace PuntoVenta.Application.DTOs.Proveedores;

public sealed record ProveedorDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Observacion { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
