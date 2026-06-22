using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record AceptarTerminosCommand(string Version) : IRequest<ErrorOr<Success>>;

public sealed class AceptarTerminosHandler(
    INegocioRepository negocioRepository,
    IFechaActual fechaActual) : IRequestHandler<AceptarTerminosCommand, ErrorOr<Success>>
{
    private readonly INegocioRepository _negocioRepository = negocioRepository;
    private readonly IFechaActual _fechaActual = fechaActual;

    public async ValueTask<ErrorOr<Success>> Handle(
        AceptarTerminosCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Version != TerminosConstants.VersionVigente)
        {
            return NegocioErrors.TerminosVersionInvalida;
        }

        var negocio = await _negocioRepository.ObtenerEditableAsync(cancellationToken);
        if (negocio is null)
        {
            return NegocioErrors.NoEncontrado;
        }

        negocio.AceptarTerminos(TerminosConstants.VersionVigente, _fechaActual.AhoraUtc);
        await _negocioRepository.UpdateAsync(negocio, cancellationToken);

        return Result.Success;
    }
}
