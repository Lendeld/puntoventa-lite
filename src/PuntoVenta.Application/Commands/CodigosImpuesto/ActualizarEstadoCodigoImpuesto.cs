using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.CodigosImpuesto;

namespace PuntoVenta.Application.Commands.CodigosImpuesto;

public sealed record ActualizarEstadoCodigoImpuestoCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoCodigoImpuestoHandler(ICodigoImpuestoRepository repository) : IRequestHandler<ActualizarEstadoCodigoImpuestoCommand, ErrorOr<bool>>
{
    private readonly ICodigoImpuestoRepository _repository = repository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoCodigoImpuestoCommand command, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return CodigoImpuestoErrors.NoEncontrado;
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
