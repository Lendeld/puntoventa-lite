using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Application.Commands.Cajas;

public sealed record ToggleCajaCommand(Guid Id) : IRequest<ErrorOr<Success>>;

public sealed class ToggleCajaHandler(
    ICajaRepository cajaRepository) : IRequestHandler<ToggleCajaCommand, ErrorOr<Success>>
{
    private readonly ICajaRepository _cajaRepository = cajaRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ToggleCajaCommand command, CancellationToken cancellationToken)
    {
        var caja = await _cajaRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (caja is null)
        {
            return CajaErrors.NoEncontrada;
        }

        if (caja.Activo)
        {
            caja.Desactivar();
        }
        else
        {
            caja.Activar();
        }

        await _cajaRepository.UpdateAsync(caja, cancellationToken);
        return Result.Success;
    }
}
