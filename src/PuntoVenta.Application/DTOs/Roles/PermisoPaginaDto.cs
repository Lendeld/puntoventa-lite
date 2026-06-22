namespace PuntoVenta.Application.DTOs.Roles;

public sealed record PermisoPaginaDto
{
    public Guid PermisoId { get; init; }
    public string Clave { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public bool Asignado { get; init; }
}
