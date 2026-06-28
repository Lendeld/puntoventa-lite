using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class ProveedorRepository(ApplicationDbContext context) : Repository<Proveedor>(context), IProveedorRepository
{
    public async Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.NombreNormalizado == nombreNormalizado, cancellationToken);

    public async Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.NombreNormalizado == nombreNormalizado && p.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Proveedor?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Proveedor> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue) query = query.Where(p => p.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(term) ||
                (p.Correo != null && p.Correo.ToLower().Contains(term)) ||
                (p.Telefono != null && p.Telefono.ToLower().Contains(term)));
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
