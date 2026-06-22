using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record ActualizarEstadoVendedorCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoVendedorHandler(IVendedorRepository vendedorRepository) : IRequestHandler<ActualizarEstadoVendedorCommand, ErrorOr<bool>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoVendedorCommand command, CancellationToken cancellationToken)
    {
        var vendedor = await _vendedorRepository.GetByIdAsync(command.Id, cancellationToken);

        if (vendedor is null)
        {
            return VendedorErrors.NoEncontrado;
        }

        if (vendedor.IsPrincipal && vendedor.Activo)
        {
            return VendedorErrors.PrincipalNoSePuedeDesactivar;
        }

        if (vendedor.Activo)
        {
            vendedor.Desactivar();
        }
        else
        {
            vendedor.Activar();
        }

        await _vendedorRepository.UpdateAsync(vendedor, cancellationToken);

        return vendedor.Activo;
    }
}
