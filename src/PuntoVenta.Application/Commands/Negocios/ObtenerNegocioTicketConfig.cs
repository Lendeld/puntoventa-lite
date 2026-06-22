using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Negocios;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record ObtenerNegocioTicketConfigQuery : IRequest<ErrorOr<NegocioTicketConfigDto>>;

public sealed class ObtenerNegocioTicketConfigHandler(
    INegocioTicketConfigRepository repository)
        : IRequestHandler<ObtenerNegocioTicketConfigQuery, ErrorOr<NegocioTicketConfigDto>>
{
    private readonly INegocioTicketConfigRepository _repository = repository;

    public async ValueTask<ErrorOr<NegocioTicketConfigDto>> Handle(
        ObtenerNegocioTicketConfigQuery query,
        CancellationToken cancellationToken)
    {
        var config = await _repository.ObtenerAsync(cancellationToken);

        if (config is null)
        {
            var creacion = NegocioTicketConfig.Crear();
            if (creacion.IsError)
            {
                return creacion.Errors;
            }

            config = await _repository.AddAsync(creacion.Value, cancellationToken);
        }

        return NegocioTicketConfigMapper.ToDto(config);
    }
}
