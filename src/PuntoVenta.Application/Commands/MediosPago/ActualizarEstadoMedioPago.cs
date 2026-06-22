using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MediosPago;

namespace PuntoVenta.Application.Commands.MediosPago;

public sealed record ActualizarEstadoMedioPagoCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoMedioPagoHandler(IMedioPagoRepository repository) : IRequestHandler<ActualizarEstadoMedioPagoCommand, ErrorOr<bool>>
{
    private readonly IMedioPagoRepository _repository = repository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoMedioPagoCommand command, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return MedioPagoErrors.NoEncontrado;
        }

        if (item.Activo)
        {
            item.Desactivar();
        }
        else
        {
            item.Activar();
        }

        await _repository.UpdateAsync(item, cancellationToken);
        return item.Activo;
    }
}
