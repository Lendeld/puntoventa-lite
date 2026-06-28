using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Application.Interfaces;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<Proveedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Proveedor> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
}
