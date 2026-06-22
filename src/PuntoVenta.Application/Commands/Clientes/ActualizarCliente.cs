using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record ActualizarClienteCommand(
    Guid Id,
    string Nombre,
    string? Identificacion = null,
    string? Correo = null,
    string? Telefono = null,
    string? Observaciones = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarClienteHandler(
    IClienteRepository clienteRepository) : IRequestHandler<ActualizarClienteCommand, ErrorOr<Success>>
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarClienteCommand command, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObtenerPorIdConAuditoriaAsync(command.Id, cancellationToken);
        if (cliente is null)
        {
            return ClienteErrors.NoEncontrado;
        }

        var identificacionNormalizada = Normalizar(command.Identificacion);
        if (identificacionNormalizada is not null &&
            await _clienteRepository.ExisteIdentificacionExcluyendoAsync(
                identificacionNormalizada, command.Id, cancellationToken))
        {
            return ClienteErrors.IdentificacionYaExiste;
        }

        var resultado = cliente.Actualizar(
            command.Nombre,
            command.Identificacion,
            command.Correo,
            command.Telefono,
            command.Observaciones);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _clienteRepository.UpdateAsync(cliente, cancellationToken);
        return Result.Success;
    }

    private static string? Normalizar(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
