using PuntoVenta.Application.DTOs.Paginas;

namespace PuntoVenta.Application.Interfaces;

public interface IUsuarioPermisoRepository
{
    Task<IReadOnlyList<string>> ObtenerClavesPermisosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaginaMenuDto>> ObtenerPaginasMenuAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
