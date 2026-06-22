using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MediosPago;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class MedioPagoRepository(ApplicationDbContext context) : Repository<MedioPago>(context), IMedioPagoRepository
{
    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(t => t.Codigo == codigo, cancellationToken);

    public async Task<HashSet<string>> ObtenerCodigosExistentesAsync(CancellationToken cancellationToken = default)
        => [.. await DbSet.Select(t => t.Codigo).ToListAsync(cancellationToken)];

    public async Task<IReadOnlyList<MedioPago>> ObtenerListaAsync(bool? activo, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activo.HasValue) query = query.Where(t => t.Activo == activo.Value);
        return await query.OrderBy(t => t.Codigo).ToListAsync(cancellationToken);
    }
}
