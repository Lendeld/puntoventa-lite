using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.TiposIdentificacion;

namespace PuntoVenta.Application.Commands.TiposIdentificacion;

public sealed record ActualizarEstadoTipoIdentificacionCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoTipoIdentificacionHandler(ITipoIdentificacionRepository tipoIdentificacionRepository) : IRequestHandler<ActualizarEstadoTipoIdentificacionCommand, ErrorOr<bool>>
{
    private readonly ITipoIdentificacionRepository _tipoIdentificacionRepository = tipoIdentificacionRepository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoTipoIdentificacionCommand command, CancellationToken cancellationToken)
    {
        var tipoIdentificacion = await _tipoIdentificacionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (tipoIdentificacion is null)
        {
            return TipoIdentificacionErrors.NoEncontrado;
        }

        if (tipoIdentificacion.Activo)
        {
            tipoIdentificacion.Desactivar();
        }
        else
        {
            tipoIdentificacion.Activar();
        }

        await _tipoIdentificacionRepository.UpdateAsync(tipoIdentificacion, cancellationToken);

        return tipoIdentificacion.Activo;
    }
}
