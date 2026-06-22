using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class VendedorRepository(ApplicationDbContext context) : Repository<Vendedor>(context), IVendedorRepository
{
    public async Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(v => v.NombreNormalizado == nombreNormalizado, cancellationToken);

    public async Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(v => v.NombreNormalizado == nombreNormalizado && v.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(v => v.Activo)
            .OrderByDescending(v => v.IsPrincipal)
            .ThenBy(v => v.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Vendedor?> ObtenerPrincipalAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(v => v.IsPrincipal && v.Activo, cancellationToken);

    public async Task<Vendedor?> ObtenerPrincipalEditableAsync(Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            v => v.IsPrincipal && (!excludeId.HasValue || v.Id != excludeId.Value),
            cancellationToken);

    public async Task<Vendedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Vendedor> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue)
            query = query.Where(v => v.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(v => v.Nombre.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(v => v.IsPrincipal)
            .ThenByDescending(v => v.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
