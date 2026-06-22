using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.TiposIdentificacion;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class TipoIdentificacionRepository(ApplicationDbContext context) : Repository<TipoIdentificacion>(context), ITipoIdentificacionRepository, IScopedService
{
    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(t => t.Codigo == codigo, cancellationToken);

    public async Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => [.. (await DbSet.Select(t => t.Codigo).ToListAsync(cancellationToken))];

    public async Task<TipoIdentificacion?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(t => t.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<TipoIdentificacion>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue)
        {
            query = query.Where(t => t.Activo == activo.Value);
        }

        return await query
            .Include(t => t.UsuarioModificacion)
            .OrderBy(t => t.Codigo)
            .ToListAsync(cancellationToken);
    }
}
