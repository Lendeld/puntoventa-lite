using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Application.Interfaces;

public interface ICajaRepository : IRepository<Caja>
{
    Task<bool> ExisteCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default);

    Task<Caja?> ObtenerPorCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default);

    Task<Caja?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Caja>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
}
