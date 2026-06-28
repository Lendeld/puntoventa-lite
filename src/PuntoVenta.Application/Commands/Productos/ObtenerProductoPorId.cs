using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Mappers;
using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.Application.Commands.Productos;

public sealed record ObtenerProductoPorIdQuery(Guid Id) : IRequest<ErrorOr<ProductoDto>>;

public sealed class ObtenerProductoPorIdHandler(
    IProductoRepository productoRepository,
    IProveedorRepository proveedorRepository) : IRequestHandler<ObtenerProductoPorIdQuery, ErrorOr<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository = productoRepository;
    private readonly IProveedorRepository _proveedorRepository = proveedorRepository;

    public async ValueTask<ErrorOr<ProductoDto>> Handle(ObtenerProductoPorIdQuery query, CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdConAuditoriaAsync(query.Id, cancellationToken);

        if (producto is null)
        {
            return ProductoErrors.NoEncontrado;
        }

        var dto = ProductoMapper.ToDto(producto);

        // Resuelve el nombre del proveedor incluso si quedó inactivo (ObtenerPorIdConAuditoria
        // no filtra por Activo), para que el detalle no lo muestre como "sin proveedor".
        if (producto.ProveedorId.HasValue)
        {
            var proveedor = await _proveedorRepository.ObtenerPorIdConAuditoriaAsync(producto.ProveedorId.Value, cancellationToken);
            if (proveedor is not null)
            {
                dto = dto with { ProveedorNombre = proveedor.Nombre };
            }
        }

        return dto;
    }
}
