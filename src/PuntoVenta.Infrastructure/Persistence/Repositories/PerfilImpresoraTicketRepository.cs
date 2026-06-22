using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Impresion;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class PerfilImpresoraTicketRepository(ApplicationDbContext context)
        : Repository<PerfilImpresoraTicket>(context), IPerfilImpresoraTicketRepository, IScopedService
{
    public Task<PerfilImpresoraTicket?> ObtenerPorClaveAsync(string clave, CancellationToken cancellationToken = default)
        => DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Clave == clave, cancellationToken);

    public async Task<bool> ExisteClaveAsync(string clave, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().AnyAsync(p => p.Clave == clave, cancellationToken);

    public async Task<IReadOnlyList<PerfilImpresoraTicket>> ListarActivosAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.AnchoMm).ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<string>> ObtenerClavesExistentesAsync(CancellationToken cancellationToken = default)
        => [.. (await DbSet.AsNoTracking().Select(p => p.Clave).ToListAsync(cancellationToken))];
}
