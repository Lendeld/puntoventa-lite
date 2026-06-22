using PuntoVenta.Domain.Entities.Impresion;

namespace PuntoVenta.Application.Interfaces;

public interface IPerfilImpresoraTicketRepository : IRepository<PerfilImpresoraTicket>
{
    Task<PerfilImpresoraTicket?> ObtenerPorClaveAsync(string clave, CancellationToken cancellationToken = default);
    Task<bool> ExisteClaveAsync(string clave, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PerfilImpresoraTicket>> ListarActivosAsync(CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerClavesExistentesAsync(CancellationToken cancellationToken = default);
}
