using ErrorOr;

namespace PuntoVenta.Application.Interfaces;

public interface IPinValidator
{
    ValueTask<ErrorOr<Success>> ValidarAsync(Guid usuarioId, string pin, CancellationToken cancellationToken = default);
}
