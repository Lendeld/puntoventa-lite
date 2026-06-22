using PuntoVenta.Application.DTOs.Categorias;
using PuntoVenta.Domain.Entities.Categorias;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class CategoriaMapper
{
    [MapperIgnoreSource(nameof(Categoria.NombreNormalizado))]
    [MapperIgnoreSource(nameof(Categoria.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Categoria.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Categoria.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Categoria.UsuarioModificacion))]
    public static partial CategoriaDto ToDto(Categoria categoria);
}
