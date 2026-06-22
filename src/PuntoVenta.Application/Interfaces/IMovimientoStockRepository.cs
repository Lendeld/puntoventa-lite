using PuntoVenta.Domain.Entities.MovimientosStock;

namespace PuntoVenta.Application.Interfaces;

public interface IMovimientoStockRepository : IRepository<MovimientoStock>
{
    Task<(IReadOnlyList<(MovimientoStock Movimiento, string NombreProducto)> Items, int Total)> ObtenerPaginadoAsync(
        Guid? productoId,
        int pagina,
        int tamano,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega los movimientos al contexto sin llamar SaveChangesAsync.
    /// Se persisten en el próximo SaveChangesAsync del contexto (ej: al guardar el documento).
    /// </summary>
    Task AgregarRangoSinPersistirAsync(IReadOnlyList<MovimientoStock> movimientos, CancellationToken cancellationToken = default);
}
