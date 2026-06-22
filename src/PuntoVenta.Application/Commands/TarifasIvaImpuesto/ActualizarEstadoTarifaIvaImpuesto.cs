using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

namespace PuntoVenta.Application.Commands.TarifasIvaImpuesto;

public sealed record ActualizarEstadoTarifaIvaImpuestoCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoTarifaIvaImpuestoHandler(ITarifaIvaImpuestoRepository repository) : IRequestHandler<ActualizarEstadoTarifaIvaImpuestoCommand, ErrorOr<bool>>
{
    private readonly ITarifaIvaImpuestoRepository _repository = repository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoTarifaIvaImpuestoCommand command, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return TarifaIvaImpuestoErrors.NoEncontrado;
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
