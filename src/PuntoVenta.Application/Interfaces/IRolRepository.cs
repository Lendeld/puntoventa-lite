using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Application.Interfaces;

public interface IRolRepository : IRepository<Rol>
{
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreExcluyendoAsync(string nombre, Guid excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rol>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaginaPermisosRolTabDto>> ObtenerPaginasConPermisosAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Rol> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
    Task<PermisosRolPorPaginaDto?> ObtenerPermisosAgrupadosPorPaginaAsync(Guid rolId, Guid paginaId, CancellationToken cancellationToken = default);
    Task ActualizarPermisosAsync(Guid rolId, Guid paginaId, IReadOnlyList<Guid> permisosIds, CancellationToken cancellationToken = default);
}
