using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Commands.Clientes;

public sealed record CrearClienteCommand(
    string Nombre,
    string? Identificacion = null,
    string? Correo = null,
    string? Telefono = null,
    string? Observaciones = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearClienteHandler(
    IClienteRepository clienteRepository) : IRequestHandler<CrearClienteCommand, ErrorOr<Guid>>
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearClienteCommand command, CancellationToken cancellationToken)
    {
        var identificacionNormalizada = Normalizar(command.Identificacion);
        if (identificacionNormalizada is not null &&
            await _clienteRepository.ExisteIdentificacionAsync(identificacionNormalizada, cancellationToken))
        {
            return ClienteErrors.IdentificacionYaExiste;
        }

        var resultado = Cliente.Crear(
            command.Nombre,
            command.Identificacion,
            command.Correo,
            command.Telefono,
            command.Observaciones);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _clienteRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }

    private static string? Normalizar(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
