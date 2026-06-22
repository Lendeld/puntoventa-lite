using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Inventario;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Queries.Inventario;

public sealed record ObtenerMovimientosStockQuery(
    Guid? ProductoId,
    int Pagina = 1,
    int Tamano = 20) : IRequest<ErrorOr<ObtenerMovimientosStockResult>>;

public sealed record ObtenerMovimientosStockResult(
    IReadOnlyList<MovimientoStockDto> Items,
    int Total);

public sealed class ObtenerMovimientosStockHandler(
    IMovimientoStockRepository movimientoRepository) : IRequestHandler<ObtenerMovimientosStockQuery, ErrorOr<ObtenerMovimientosStockResult>>
{
    private readonly IMovimientoStockRepository _movimientoRepository = movimientoRepository;

    public async ValueTask<ErrorOr<ObtenerMovimientosStockResult>> Handle(ObtenerMovimientosStockQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _movimientoRepository.ObtenerPaginadoAsync(
            query.ProductoId,
            query.Pagina,
            query.Tamano,
            cancellationToken);

        var dtos = items.Select(x => new MovimientoStockDto(
            x.Movimiento.Id,
            x.Movimiento.ProductoId,
            x.NombreProducto,
            x.Movimiento.FechaUtc,
            x.Movimiento.TipoDocumentoOrigen,
            x.Movimiento.DocumentoVentaId,
            x.Movimiento.ConsecutivoDocumento,
            x.Movimiento.Delta,
            x.Movimiento.SaldoResultante,
            x.Movimiento.UsuarioId,
            x.Movimiento.Razon)).ToList();

        return new ObtenerMovimientosStockResult(dtos, total);
    }
}
