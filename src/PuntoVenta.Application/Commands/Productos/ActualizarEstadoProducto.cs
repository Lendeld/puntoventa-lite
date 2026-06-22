using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ActualizarEstadoProductoCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoProductoHandler(IProductoRepository productoRepository) : IRequestHandler<ActualizarEstadoProductoCommand, ErrorOr<bool>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoProductoCommand command, CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.GetByIdAsync(command.Id, cancellationToken);

        if (producto is null)
        {
            return ProductoErrors.NoEncontrado;
        }

        if (producto.Activo)
        {
            producto.Desactivar();
        }
        else
        {
            producto.Activar();
        }

        await _productoRepository.UpdateAsync(producto, cancellationToken);

        return producto.Activo;
    }
}
