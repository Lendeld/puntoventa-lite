using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Proveedores;

namespace PuntoVenta.Application.Commands.Proveedores;

public sealed record ActualizarProveedorCommand(
    Guid Id,
    string Nombre,
    string? Correo = null,
    string? Telefono = null,
    string? Observacion = null,
    bool Activo = true) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarProveedorHandler(IProveedorRepository proveedorRepository) : IRequestHandler<ActualizarProveedorCommand, ErrorOr<Success>>
{
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarProveedorCommand command, CancellationToken cancellationToken)
    {
        var proveedor = await _proveedorRepository.ObtenerPorIdConAuditoriaAsync(command.Id, cancellationToken);

        if (proveedor is null)
        {
            return ProveedorErrors.NoEncontrado;
        }

        var nombreNormalizado = Proveedor.NormalizarNombre(command.Nombre);
        if (await _proveedorRepository.ExisteNombreExcluyendoAsync(nombreNormalizado, command.Id, cancellationToken))
        {
            return ProveedorErrors.NombreYaExiste;
        }

        var resultado = proveedor.Actualizar(command.Nombre, command.Correo, command.Telefono, command.Observacion);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        if (command.Activo && !proveedor.Activo)
        {
            proveedor.Activar();
        }
        else if (!command.Activo && proveedor.Activo)
        {
            proveedor.Desactivar();
        }

        await _proveedorRepository.UpdateAsync(proveedor, cancellationToken);

        return Result.Success;
    }
}
