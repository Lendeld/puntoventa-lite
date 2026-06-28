using PuntoVenta.Application.DTOs.Proveedores;
using PuntoVenta.Domain.Entities.Proveedores;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class ProveedorMapper
{
    [MapperIgnoreSource(nameof(Proveedor.NombreNormalizado))]
    [MapperIgnoreSource(nameof(Proveedor.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Proveedor.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Proveedor.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Proveedor.UsuarioModificacion))]
    public static partial ProveedorDto ToDto(Proveedor proveedor);
}
