using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Interfaces;

public interface IDocumentoVentaEventoRepository : IRepository<DocumentoVentaEvento>
{
    Task AgregarSinPersistirAsync(DocumentoVentaEvento evento, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<DocumentoVentaEvento> Items, int Total)> ObtenerPorDocumentoAsync(
        Guid documentoVentaId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteTipoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<TipoDocumentoVentaEvento?> ObtenerTipoAsync(string codigo, CancellationToken cancellationToken = default);
}
