namespace PuntoVenta.Application.DTOs.Roles;

public sealed record PaginaPermisosRolTabDto
{
    public Guid PaginaId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public int Orden { get; init; }
}
