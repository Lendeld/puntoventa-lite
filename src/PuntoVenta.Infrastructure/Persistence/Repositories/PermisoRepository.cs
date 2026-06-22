using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class PermisoRepository(ApplicationDbContext context) : Repository<Permiso>(context), IPermisoRepository, IScopedService
{
    public async Task<bool> ExisteClaveAsync(string clave, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.Clave == clave, cancellationToken);

    public async Task<Permiso?> ObtenerPorClaveAsync(string clave, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Clave == clave, cancellationToken);

    public async Task<IReadOnlyList<Permiso>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Clave)
            .ToListAsync(cancellationToken);
}
