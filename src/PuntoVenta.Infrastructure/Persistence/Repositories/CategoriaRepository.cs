using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class CategoriaRepository(ApplicationDbContext context) : Repository<Categoria>(context), ICategoriaRepository
{
    public async Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.NombreNormalizado == nombreNormalizado, cancellationToken);

    public async Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.NombreNormalizado == nombreNormalizado && c.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Categoria>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Categoria?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Categoria> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue) query = query.Where(c => c.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(term) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
