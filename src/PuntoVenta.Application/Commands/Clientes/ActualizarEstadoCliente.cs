using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record ActualizarEstadoClienteCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoClienteHandler(
    IClienteRepository clienteRepository) : IRequestHandler<ActualizarEstadoClienteCommand, ErrorOr<bool>>
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoClienteCommand command, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(command.Id, cancellationToken);
        if (cliente is null)
        {
            return ClienteErrors.NoEncontrado;
        }

        if (cliente.Activo)
        {
            cliente.Desactivar();
        }
        else
        {
            cliente.Activar();
        }

        await _clienteRepository.UpdateAsync(cliente, cancellationToken);
        return cliente.Activo;
    }
}
