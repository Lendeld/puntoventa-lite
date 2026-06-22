using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class TarifaIvaImpuestoRepository(ApplicationDbContext context) : Repository<TarifaIvaImpuesto>(context), ITarifaIvaImpuestoRepository
{
    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(t => t.Codigo == codigo, cancellationToken);

    public async Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => [.. await DbSet.Select(t => t.Codigo).ToListAsync(cancellationToken)];

    public async Task<IReadOnlyList<TarifaIvaImpuesto>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activo.HasValue) query = query.Where(t => t.Activo == activo.Value);
        return await query.OrderBy(t => t.Codigo).ToListAsync(cancellationToken);
    }
}
