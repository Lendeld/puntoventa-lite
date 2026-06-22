using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record ActualizarVendedorCommand(
    Guid Id,
    string Nombre,
    bool IsPrincipal = false,
    bool Activo = true) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarVendedorHandler(IVendedorRepository vendedorRepository) : IRequestHandler<ActualizarVendedorCommand, ErrorOr<Success>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarVendedorCommand command, CancellationToken cancellationToken)
    {
        var vendedor = await _vendedorRepository.GetByIdAsync(command.Id, cancellationToken);

        if (vendedor is null)
        {
            return VendedorErrors.NoEncontrado;
        }

        var nombreNormalizado = Vendedor.NormalizarNombre(command.Nombre);
        if (await _vendedorRepository.ExisteNombreExcluyendoAsync(nombreNormalizado, command.Id, cancellationToken))
        {
            return VendedorErrors.NombreYaExiste;
        }

        if (vendedor.IsPrincipal && !command.IsPrincipal)
        {
            return VendedorErrors.PrincipalNoSePuedeQuitarSinReemplazo;
        }

        if (vendedor.IsPrincipal && !command.Activo)
        {
            return VendedorErrors.PrincipalNoSePuedeDesactivar;
        }

        var principalActual = await _vendedorRepository.ObtenerPrincipalEditableAsync(command.Id, cancellationToken);

        if (command.IsPrincipal && principalActual is not null)
        {
            principalActual.QuitarPrincipal();
        }

        var resultado = vendedor.Actualizar(command.Nombre, command.IsPrincipal);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        if (command.Activo && !vendedor.Activo)
        {
            vendedor.Activar();
        }
        else if (!command.Activo && vendedor.Activo)
        {
            vendedor.Desactivar();
        }

        await _vendedorRepository.UpdateAsync(vendedor, cancellationToken);

        return Result.Success;
    }
}
