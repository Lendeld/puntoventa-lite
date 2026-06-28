using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Application.Commands.Proveedores;

public sealed record CrearProveedorCommand(
    string Nombre,
    string? Correo = null,
    string? Telefono = null,
    string? Observacion = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearProveedorHandler(IProveedorRepository proveedorRepository) : IRequestHandler<CrearProveedorCommand, ErrorOr<Guid>>
{
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearProveedorCommand command, CancellationToken cancellationToken)
    {
        var nombreNormalizado = Proveedor.NormalizarNombre(command.Nombre);
        if (await _proveedorRepository.ExisteNombreAsync(nombreNormalizado, cancellationToken))
        {
            return ProveedorErrors.NombreYaExiste;
        }

        var resultado = Proveedor.Crear(command.Nombre, command.Correo, command.Telefono, command.Observacion);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _proveedorRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }
}
