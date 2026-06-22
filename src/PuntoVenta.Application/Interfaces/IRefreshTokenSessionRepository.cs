using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Application.Interfaces;

public interface IRefreshTokenSessionRepository
{
    Task<RefreshTokenSession?> ObtenerPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshTokenSession?> ObtenerReemplazoPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AgregarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default);
    Task ActualizarAsync(RefreshTokenSession session, CancellationToken cancellationToken = default);
    Task RevocarSesionesUsuarioAsync(
        Guid usuarioId,
        DateTime revocadoEnUtc,
        CancellationToken cancellationToken = default);
    Task EliminarExpiradosAsync(CancellationToken cancellationToken = default);
}
