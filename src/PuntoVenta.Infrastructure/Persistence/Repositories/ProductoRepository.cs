using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.DTOs.Inventarios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;
using PuntoVenta.Domain.Entities.Productos;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

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

    public async Task<IReadOnlyList<InventarioReporteProyeccionDto>> ObtenerReporteInventarioProyectadoAsync(
        string? codigo,
        Guid? categoriaId,
        int maxFilas,
        CancellationToken cancellationToken = default)
    {
        // Universo fijo: Bienes activos que manejan existencia.
        var query = DbSet.AsNoTracking()
            .Where(p => p.TipoItem == TipoItem.Bien && !p.NoAplicaExistencias && p.Activo);

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var term = codigo.Trim().ToLower();
            query = query.Where(p => p.Codigo.ToLower().Contains(term));
        }

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        // Lite es single-tenant: las subconsultas NO llevan filtro de tenant.
        return await query
            .OrderBy(p => p.Codigo)
            .Take(maxFilas + 1)
            .Select(p => new InventarioReporteProyeccionDto
            {
                ProductoId = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Categoria = Context.Set<Categoria>()
                    .Where(c => c.Id == p.CategoriaId)
                    .Select(c => c.Nombre)
                    .FirstOrDefault(),
                FechaCreacion = p.FechaCreacion,
                Existencia = p.Existencia,
                PrecioUnitario = p.PrecioUnitario,
                PrecioCosto = p.PrecioCosto,
                TarifaPorcentaje = Context.Set<TarifaIvaImpuesto>()
                    .Where(t => t.Codigo == p.TarifaIvaImpuestoCodigo)
                    .Select(t => (decimal?)t.Porcentaje)
                    .FirstOrDefault() ?? 0m,
            })
            .ToListAsync(cancellationToken);
    }
}
