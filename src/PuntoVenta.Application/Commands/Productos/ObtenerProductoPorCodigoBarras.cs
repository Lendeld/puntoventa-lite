using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ObtenerProductoPorCodigoBarrasQuery(string CodigoBarras) : IRequest<ErrorOr<ProductoDto>>;

public sealed class ObtenerProductoPorCodigoBarrasHandler(
    IProductoRepository productoRepository) : IRequestHandler<ObtenerProductoPorCodigoBarrasQuery, ErrorOr<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;

    public async ValueTask<ErrorOr<ProductoDto>> Handle(ObtenerProductoPorCodigoBarrasQuery query, CancellationToken cancellationToken)
    {
        var codigoBarras = query.CodigoBarras?.Trim();
        if (string.IsNullOrEmpty(codigoBarras))
        {
            return ProductoErrors.NoEncontrado;
        }

        var producto = await _productoRepository.ObtenerPorCodigoBarrasAsync(codigoBarras, cancellationToken);
        if (producto is null)
        {
            return ProductoErrors.NoEncontrado;
        }

        return ProductoMapper.ToDto(producto);
    }
}
