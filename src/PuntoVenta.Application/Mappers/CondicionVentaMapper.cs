using PuntoVenta.Application.DTOs.CondicionesVenta;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class CondicionVentaMapper
{
    [MapperIgnoreTarget(nameof(CondicionVentaDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(CondicionVenta.FechaCreacion))]
    [MapperIgnoreSource(nameof(CondicionVenta.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(CondicionVenta.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(CondicionVenta.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(CondicionVenta.UsuarioModificacionId))]
    private static partial CondicionVentaDto ToDtoGenerated(CondicionVenta condicionVenta);

    public static CondicionVentaDto ToDto(CondicionVenta condicionVenta)
        => ToDtoGenerated(condicionVenta) with
        {
            ModificadoPor = condicionVenta.UsuarioModificacion?.Nombre,
        };
}
