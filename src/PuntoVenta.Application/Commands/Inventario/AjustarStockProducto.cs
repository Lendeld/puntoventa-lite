using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.MovimientosStock;

namespace PuntoVenta.Application.Commands.Inventario;

public sealed record AjustarStockProductoCommand(
    Guid ProductoId,
    decimal Delta,
    string? Razon = null) : IRequest<ErrorOr<Guid>>;

public sealed class AjustarStockProductoHandler(
    IProductoRepository productoRepository,
    IMovimientoStockRepository movimientoRepository,
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual) : IRequestHandler<AjustarStockProductoCommand, ErrorOr<Guid>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly IMovimientoStockRepository _movimientoRepository = movimientoRepository;
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;

    public async ValueTask<ErrorOr<Guid>> Handle(AjustarStockProductoCommand command, CancellationToken cancellationToken)
    {
        if (command.Delta == 0)
            return MovimientoStockErrors.DeltaCero;

        var producto = await _productoRepository.ObtenerEditableAsync(command.ProductoId, cancellationToken);
        if (producto is null)
            return MovimientoStockErrors.ProductoNoEncontrado;

        var saldoResultante = producto.AplicarMovimientoStock(command.Delta);

        var movimiento = MovimientoStock.Crear(
            productoId: producto.Id,
            fechaUtc: _fechaActual.AhoraUtc,
            delta: command.Delta,
            saldoResultante: saldoResultante,
            usuarioId: _usuarioActual.UsuarioId,
            razon: command.Razon);

        if (movimiento.IsError)
            return movimiento.Errors;

        await _productoRepository.UpdateAsync(producto, cancellationToken);
        await _movimientoRepository.AddAsync(movimiento.Value, cancellationToken);

        return movimiento.Value.Id;
    }
}
