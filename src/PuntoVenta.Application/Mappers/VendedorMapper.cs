using PuntoVenta.Application.DTOs.Vendedores;
using PuntoVenta.Domain.Entities.Vendedores;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class VendedorMapper
{
    [MapperIgnoreSource(nameof(Vendedor.NombreNormalizado))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioModificacion))]
    public static partial VendedorDto ToDto(Vendedor vendedor);

    [MapperIgnoreSource(nameof(Vendedor.NombreNormalizado))]
    [MapperIgnoreSource(nameof(Vendedor.Activo))]
    [MapperIgnoreSource(nameof(Vendedor.FechaCreacion))]
    [MapperIgnoreSource(nameof(Vendedor.FechaModificacion))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Vendedor.UsuarioModificacion))]
    public static partial VendedorActivoDto ToActivoDto(Vendedor vendedor);
}
