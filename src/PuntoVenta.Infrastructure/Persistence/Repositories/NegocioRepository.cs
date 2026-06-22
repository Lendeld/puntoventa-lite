using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class NegocioRepository(ApplicationDbContext context) : Repository<Negocio>(context), INegocioRepository
{
    public async Task<Negocio?> ObtenerAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<Negocio?> ObtenerEditableAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsTracking().FirstOrDefaultAsync(cancellationToken);
}
