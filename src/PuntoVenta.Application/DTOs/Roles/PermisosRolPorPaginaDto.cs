namespace PuntoVenta.Application.DTOs.Roles;

public sealed record PermisosRolPorPaginaDto
{
    public Guid PaginaId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public IReadOnlyList<PermisoPaginaDto> Permisos { get; init; } = [];
}
