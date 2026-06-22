using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record ObtenerClientePorIdQuery(Guid Id) : IRequest<ErrorOr<ClienteDto>>;

public sealed class ObtenerClientePorIdHandler(IClienteRepository clienteRepository) : IRequestHandler<ObtenerClientePorIdQuery, ErrorOr<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async ValueTask<ErrorOr<ClienteDto>> Handle(ObtenerClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);
        if (cliente is null)
        {
            return ClienteErrors.NoEncontrado;
        }

        return ClienteMapper.ToDto(cliente);
    }
}
