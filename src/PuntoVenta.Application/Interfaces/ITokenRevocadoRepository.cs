using PuntoVenta.Domain.Entities.Tokens;

namespace PuntoVenta.Application.Interfaces;

public interface ITokenRevocadoRepository
{
    Task RevocarAsync(TokenRevocado token, CancellationToken cancellationToken = default);
    Task<bool> EstaRevocadoAsync(string jti, CancellationToken cancellationToken = default);
    Task EliminarExpiradosAsync(CancellationToken cancellationToken = default);
}
