using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Secuencias;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class SecuenciaRepository(ApplicationDbContext context)
    : Repository<Secuencia>(context), ISecuenciaRepository, IScopedService
{
    public async Task<Secuencia> ObtenerOCrearEditableAsync(
        TipoDocumentoVenta tipoDocumento,
        CancellationToken cancellationToken = default)
    {
        var secuencia = await DbSet
            .AsTracking()
            .FirstOrDefaultAsync(s => s.TipoDocumento == tipoDocumento, cancellationToken);

        if (secuencia is not null)
        {
            return secuencia;
        }

        var nueva = Secuencia.Crear(tipoDocumento).Value;
        await DbSet.AddAsync(nueva, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return nueva;
    }
}
