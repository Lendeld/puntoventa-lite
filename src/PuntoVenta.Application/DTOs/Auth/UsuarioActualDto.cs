namespace PuntoVenta.Application.DTOs.Auth;

public sealed record UsuarioActualDto
{
    public string Usuario { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Identificacion { get; init; } = string.Empty;
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public bool TienePin { get; init; }
    public bool DebeCambiarPassword { get; init; }
}
