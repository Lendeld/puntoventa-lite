using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Application.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<bool> ExisteNombreAsync(string nombreNormalizado, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreExcluyendoAsync(string nombreNormalizado, Guid excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Categoria>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<Categoria?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Categoria> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
}
