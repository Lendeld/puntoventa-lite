namespace PuntoVenta.Application.DTOs.Usuarios;

public sealed record UsuarioDto
{
    public Guid Id { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Identificacion { get; init; } = string.Empty;
    public Guid? RolId { get; init; }
    public string? RolNombre { get; init; }
    public bool EsPropietario { get; init; }
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public bool TienePin { get; init; }
    public bool DebeCambiarPassword { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaModificacion { get; init; }
    public string? CreadoPor { get; init; }
    public string? ModificadoPor { get; init; }
}
