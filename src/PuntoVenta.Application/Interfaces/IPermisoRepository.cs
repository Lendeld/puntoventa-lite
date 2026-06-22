using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.Application.Interfaces;

public interface IPermisoRepository : IRepository<Permiso>
{
    Task<bool> ExisteClaveAsync(string clave, CancellationToken cancellationToken = default);
    Task<Permiso?> ObtenerPorClaveAsync(string clave, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permiso>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
}
