using PuntoVenta.Domain.Entities.TiposIdentificacion;

namespace PuntoVenta.Application.Interfaces;

public interface ITipoIdentificacionRepository : IRepository<TipoIdentificacion>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default);
    Task<TipoIdentificacion?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TipoIdentificacion>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default);
}
