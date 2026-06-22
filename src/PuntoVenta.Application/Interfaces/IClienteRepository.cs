using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);
    Task<bool> ExisteIdentificacionExcluyendoAsync(string identificacion, Guid excludeId, CancellationToken cancellationToken = default);
    Task<Cliente?> ObtenerPorIdConAuditoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Cliente> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        bool? activo,
        CancellationToken cancellationToken = default);
}
