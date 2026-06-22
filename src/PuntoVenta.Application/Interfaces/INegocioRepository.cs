using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Interfaces;

public interface INegocioRepository : IRepository<Negocio>
{
    Task<Negocio?> ObtenerAsync(CancellationToken cancellationToken = default);
    Task<Negocio?> ObtenerEditableAsync(CancellationToken cancellationToken = default);
}
