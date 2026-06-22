using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record EstadoTerminosDto(
    bool Aceptado,
    string VersionVigente,
    string? VersionAceptada,
    DateTime? FechaAceptacionUtc);

public sealed record ObtenerEstadoTerminosQuery() : IRequest<ErrorOr<EstadoTerminosDto>>;

public sealed class ObtenerEstadoTerminosHandler(INegocioRepository negocioRepository)
    : IRequestHandler<ObtenerEstadoTerminosQuery, ErrorOr<EstadoTerminosDto>>
{
    private readonly INegocioRepository _negocioRepository = negocioRepository;

    public async ValueTask<ErrorOr<EstadoTerminosDto>> Handle(
        ObtenerEstadoTerminosQuery query,
        CancellationToken cancellationToken)
    {
        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        var versionAceptada = negocio?.TerminosAceptadosVersion;
        var aceptado = versionAceptada == TerminosConstants.VersionVigente;

        return new EstadoTerminosDto(
            aceptado,
            TerminosConstants.VersionVigente,
            versionAceptada,
            negocio?.TerminosAceptadosFechaUtc);
    }
}
