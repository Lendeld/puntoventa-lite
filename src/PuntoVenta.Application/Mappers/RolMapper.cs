using PuntoVenta.Application.DTOs.Roles;
using PuntoVenta.Domain.Entities.Roles;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class RolMapper
{
    [MapperIgnoreSource(nameof(Rol.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Rol.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Rol.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Rol.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(Rol.RolPermisos))]
    public static partial RolDto ToDto(Rol rol);
}
