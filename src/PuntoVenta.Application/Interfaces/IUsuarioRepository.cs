using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
    Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreUsuarioExcluyendoAsync(string nombreUsuario, Guid excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorIdDelNegocioAsync(Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);
    Task<Usuario?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
}
