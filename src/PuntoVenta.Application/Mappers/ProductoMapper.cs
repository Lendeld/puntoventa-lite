using PuntoVenta.Application.DTOs.Productos;
using PuntoVenta.Domain.Entities.Productos;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class ProductoMapper
{
    [MapperIgnoreSource(nameof(Producto.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Producto.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Producto.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Producto.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(Producto.Activo))]
    [MapProperty(nameof(Producto.Existencia), nameof(ProductoDto.ExistenciaTotal))]
    public static partial ProductoDto ToDto(Producto producto);
}
