using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Interfaces;

public interface INegocioTicketConfigRepository : IRepository<NegocioTicketConfig>
{
    Task<NegocioTicketConfig?> ObtenerAsync(CancellationToken cancellationToken = default);
    Task<NegocioTicketConfig?> ObtenerEditableAsync(CancellationToken cancellationToken = default);
}
