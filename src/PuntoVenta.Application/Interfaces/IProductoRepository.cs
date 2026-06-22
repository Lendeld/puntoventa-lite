using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoExcluyendoAsync(string codigo, Guid excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoBarrasExcluyendoAsync(string codigoBarras, Guid excludeId, CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        TipoItem? tipoItem,
        Guid? categoriaId,
        CancellationToken cancellationToken = default);
}
