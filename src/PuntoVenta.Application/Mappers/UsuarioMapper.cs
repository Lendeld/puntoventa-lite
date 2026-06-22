using PuntoVenta.Application.DTOs.Usuarios;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class UsuarioMapper
{
    [MapperIgnoreTarget(nameof(UsuarioDto.RolNombre))]
    [MapperIgnoreTarget(nameof(UsuarioDto.CreadoPor))]
    [MapperIgnoreTarget(nameof(UsuarioDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(Usuario.PasswordHash))]
    [MapperIgnoreSource(nameof(Usuario.PasswordTemporalExpiraEnUtc))]
    [MapperIgnoreSource(nameof(Usuario.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Usuario.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Usuario.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(Usuario.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Usuario.PinHash))]
    private static partial UsuarioDto ToDtoGenerated(Usuario usuario);

    public static UsuarioDto ToDto(Usuario usuario, Rol? rol = null)
    {
        return ToDtoGenerated(usuario) with
        {
            RolNombre = rol?.Nombre,
            CreadoPor = usuario.UsuarioCreacion?.Nombre,
            ModificadoPor = usuario.UsuarioModificacion?.Nombre,
        };
    }
}
