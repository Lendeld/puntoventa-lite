using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class NegocioTicketConfigRepository(ApplicationDbContext context)
    : Repository<NegocioTicketConfig>(context), INegocioTicketConfigRepository
{
    public async Task<NegocioTicketConfig?> ObtenerAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<NegocioTicketConfig?> ObtenerEditableAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsTracking().FirstOrDefaultAsync(cancellationToken);
}
