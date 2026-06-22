using PuntoVenta.Application.DTOs.TiposIdentificacion;
using PuntoVenta.Domain.Entities.TiposIdentificacion;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class TipoIdentificacionMapper
{
    [MapperIgnoreTarget(nameof(TipoIdentificacionDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(TipoIdentificacion.FechaCreacion))]
    [MapperIgnoreSource(nameof(TipoIdentificacion.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(TipoIdentificacion.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(TipoIdentificacion.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(TipoIdentificacion.UsuarioModificacionId))]
    private static partial TipoIdentificacionDto ToDtoGenerated(
        TipoIdentificacion tipoIdentificacion
    );

    public static TipoIdentificacionDto ToDto(
        TipoIdentificacion tipoIdentificacion
    ) => ToDtoGenerated(tipoIdentificacion) with
    {
        ModificadoPor = tipoIdentificacion.UsuarioModificacion?.Nombre,
    };
}
