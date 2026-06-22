using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Negocios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record ObtenerNegocioQuery : IRequest<ErrorOr<NegocioDto>>;

public sealed class ObtenerNegocioHandler(INegocioRepository negocioRepository) : IRequestHandler<ObtenerNegocioQuery, ErrorOr<NegocioDto>>
{
    private readonly INegocioRepository _negocioRepository = negocioRepository;

    public async ValueTask<ErrorOr<NegocioDto>> Handle(ObtenerNegocioQuery query, CancellationToken cancellationToken)
    {
        var negocio = await _negocioRepository.ObtenerAsync(cancellationToken);
        if (negocio is null)
        {
            return NegocioErrors.NoEncontrado;
        }

        return NegocioMapper.ToDto(negocio);
    }
}
