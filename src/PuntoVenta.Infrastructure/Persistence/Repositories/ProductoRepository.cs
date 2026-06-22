using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class ProductoRepository(ApplicationDbContext context) : Repository<Producto>(context), IProductoRepository
{
    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.Codigo == codigo, cancellationToken);

    public async Task<bool> ExisteCodigoExcluyendoAsync(string codigo, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.Codigo == codigo && p.Id != excludeId, cancellationToken);

    public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.CodigoBarras == codigoBarras, cancellationToken);

    public async Task<bool> ExisteCodigoBarrasExcluyendoAsync(string codigoBarras, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.CodigoBarras == codigoBarras && p.Id != excludeId, cancellationToken);

    public async Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras, cancellationToken);

    public async Task<Producto?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Producto>> ObtenerPorIdsEditablesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        => await DbSet.AsTracking().Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task<Producto?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Producto> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        TipoItem? tipoItem,
        Guid? categoriaId,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (tipoItem.HasValue)
            query = query.Where(p => p.TipoItem == tipoItem.Value);

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(p =>
                p.Codigo.ToLower().Contains(term) ||
                p.Nombre.ToLower().Contains(term) ||
                (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
