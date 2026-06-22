namespace PuntoVenta.Application.DTOs.Clientes;

public sealed record ClienteDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Identificacion { get; init; }
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Observaciones { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
}
