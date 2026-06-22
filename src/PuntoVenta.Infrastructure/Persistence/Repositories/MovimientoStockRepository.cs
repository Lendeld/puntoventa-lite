using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MovimientosStock;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class MovimientoStockRepository(ApplicationDbContext context)
    : Repository<MovimientoStock>(context), IMovimientoStockRepository, IScopedService
{
    public async Task AgregarRangoSinPersistirAsync(IReadOnlyList<MovimientoStock> movimientos, CancellationToken cancellationToken = default)
    {
        if (movimientos.Count > 0)
            await DbSet.AddRangeAsync(movimientos, cancellationToken);
    }

    public async Task<(IReadOnlyList<(MovimientoStock Movimiento, string NombreProducto)> Items, int Total)> ObtenerPaginadoAsync(
        Guid? productoId,
        int pagina,
        int tamano,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (productoId.HasValue)
            query = query.Where(m => m.ProductoId == productoId.Value);

        query = query.OrderByDescending(m => m.FechaUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Join(
                Context.Set<Producto>().AsNoTracking(),
                m => m.ProductoId,
                p => p.Id,
                (m, p) => new { Movimiento = m, NombreProducto = p.Nombre })
            .ToListAsync(cancellationToken);

        return ([.. items.Select(x => (x.Movimiento, x.NombreProducto))], total);
    }
}
