using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Application.Interfaces;

public interface IVendedorRepository : IRepository<Vendedor>
{
    Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<Vendedor?> ObtenerPrincipalAsync(CancellationToken cancellationToken = default);
    Task<Vendedor?> ObtenerPrincipalEditableAsync(Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Vendedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vendedor> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
}
