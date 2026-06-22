using PuntoVenta.Domain.Entities.MediosPago;

namespace PuntoVenta.Application.Interfaces;

public interface IMedioPagoRepository : IRepository<MedioPago>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedioPago>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default);
}
