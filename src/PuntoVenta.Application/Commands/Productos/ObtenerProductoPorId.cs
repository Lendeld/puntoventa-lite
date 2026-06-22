using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ObtenerProductoPorIdQuery(Guid Id) : IRequest<ErrorOr<ProductoDto>>;

public sealed class ObtenerProductoPorIdHandler(
    IProductoRepository productoRepository) : IRequestHandler<ObtenerProductoPorIdQuery, ErrorOr<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;

    public async ValueTask<ErrorOr<ProductoDto>> Handle(ObtenerProductoPorIdQuery query, CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);

        if (producto is null)
        {
            return ProductoErrors.NoEncontrado;
        }

        return ProductoMapper.ToDto(producto);
    }
}
