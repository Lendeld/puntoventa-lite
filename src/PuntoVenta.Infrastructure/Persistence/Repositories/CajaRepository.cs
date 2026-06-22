using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class CajaRepository(ApplicationDbContext context) : Repository<Caja>(context), ICajaRepository
{
    public async Task<bool> ExisteCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.CodigoNormalizado == codigoNormalizado, cancellationToken);

    public async Task<Caja?> ObtenerPorCodigoAsync(string codigoNormalizado, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodigoNormalizado == codigoNormalizado, cancellationToken);

    public async Task<Caja?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Caja>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync(cancellationToken);
}
