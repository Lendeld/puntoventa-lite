using PuntoVenta.Domain.Entities.CodigosImpuesto;

namespace PuntoVenta.Application.Interfaces;

public interface ICodigoImpuestoRepository : IRepository<CodigoImpuesto>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CodigoImpuesto>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default);
}
