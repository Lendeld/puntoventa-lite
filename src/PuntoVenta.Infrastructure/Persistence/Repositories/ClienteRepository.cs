using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository(ApplicationDbContext context) : Repository<Cliente>(context), IClienteRepository
{
    public async Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.Identificacion == identificacion, cancellationToken);

    public async Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.Identificacion == identificacion && c.Id != excludeId, cancellationToken);

    public async Task<Cliente?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Cliente> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (activo.HasValue) query = query.Where(c => c.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(term) ||
                (c.Identificacion != null && c.Identificacion.ToLower().Contains(term)) ||
                (c.Correo != null && c.Correo.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
