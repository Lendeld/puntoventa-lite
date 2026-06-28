using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    // Cap de filas del reporte de inventario. El repo lee a lo sumo este valor + 1;
    // el handler rechaza si la cantidad supera el tope.
    const int MaxFilasReporteInventario = 100_000;

    // Proyección liviana para el reporte de inventario (una fila por producto).
    Task<IReadOnlyList<InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(
        string? codigo,
        Guid? categoriaId,
        Guid? proveedorId,
        int maxFilas,
        CancellationToken cancellationToken = default);

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
