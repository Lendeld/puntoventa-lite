using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Application.Commands.Cajas;

public sealed record ActualizarCajaCommand(Guid Id, string? Codigo, string? Nombre) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarCajaHandler(
    ICajaRepository cajaRepository) : IRequestHandler<ActualizarCajaCommand, ErrorOr<Success>>
{
    private readonly ICajaRepository _cajaRepository = cajaRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarCajaCommand command, CancellationToken cancellationToken)
    {
        var caja = await _cajaRepository.ObtenerEditableAsync(command.Id, cancellationToken);
        if (caja is null)
        {
            return CajaErrors.NoEncontrada;
        }

        var codigoNuevo = Caja.NormalizarCodigo(command.Codigo ?? string.Empty);
        if (caja.CodigoNormalizado != codigoNuevo &&
            await _cajaRepository.ExisteCodigoAsync(codigoNuevo, cancellationToken))
        {
            return CajaErrors.CodigoYaExiste;
        }

        var resultado = caja.Actualizar(command.Codigo ?? string.Empty, command.Nombre ?? string.Empty);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _cajaRepository.UpdateAsync(caja, cancellationToken);
        return Result.Success;
    }
}
