using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.CondicionesVenta;

namespace PuntoVenta.Application.Commands.CondicionesVenta;

public sealed record ActualizarEstadoCondicionVentaCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoCondicionVentaHandler(ICondicionVentaRepository repository) : IRequestHandler<ActualizarEstadoCondicionVentaCommand, ErrorOr<bool>>
{
    private readonly ICondicionVentaRepository _repository = repository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoCondicionVentaCommand command, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return CondicionVentaErrors.NoEncontrado;
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
