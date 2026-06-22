using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Cajas;

namespace PuntoVenta.Application.Commands.Cajas;

public sealed record CrearCajaCommand(string? Codigo, string? Nombre) : IRequest<ErrorOr<Guid>>;

public sealed class CrearCajaHandler(
    ICajaRepository cajaRepository) : IRequestHandler<CrearCajaCommand, ErrorOr<Guid>>
{
    private readonly ICajaRepository _cajaRepository = cajaRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearCajaCommand command, CancellationToken cancellationToken)
    {
        var codigoNormalizado = Caja.NormalizarCodigo(command.Codigo ?? string.Empty);
        if (await _cajaRepository.ExisteCodigoAsync(codigoNormalizado, cancellationToken))
        {
            return CajaErrors.CodigoYaExiste;
        }

        var resultado = Caja.Crear(command.Codigo ?? string.Empty, command.Nombre ?? string.Empty);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _cajaRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }
}
