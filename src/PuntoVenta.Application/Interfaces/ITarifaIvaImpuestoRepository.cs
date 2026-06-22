using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

namespace PuntoVenta.Application.Interfaces;

public interface ITarifaIvaImpuestoRepository : IRepository<TarifaIvaImpuesto>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TarifaIvaImpuesto>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default);
}
