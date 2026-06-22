using PuntoVenta.Domain.Entities.CondicionesVenta;

namespace PuntoVenta.Application.Interfaces;

public interface ICondicionVentaRepository : IRepository<CondicionVenta>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CondicionVenta>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default);
}
