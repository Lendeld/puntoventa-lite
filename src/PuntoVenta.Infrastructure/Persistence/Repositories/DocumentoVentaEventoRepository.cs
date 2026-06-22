using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class DocumentoVentaEventoRepository(ApplicationDbContext context)
    : Repository<DocumentoVentaEvento>(context), IDocumentoVentaEventoRepository
{
    public async Task AgregarSinPersistirAsync(DocumentoVentaEvento evento, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(evento, cancellationToken);

    public async Task<(IReadOnlyList<DocumentoVentaEvento> Items, int Total)> ObtenerPorDocumentoAsync(
        Guid documentoVentaId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(e => e.DocumentoVentaId == documentoVentaId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.OcurridoEn)
            .ThenByDescending(e => e.FechaCreacion)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    // Sin catálogo de tipos en la versión lite; todos los tipos son válidos.
    public Task<bool> ExisteTipoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<TipoDocumentoVentaEvento?> ObtenerTipoAsync(string codigo, CancellationToken cancellationToken = default)
        => Task.FromResult<TipoDocumentoVentaEvento?>(null);
}
