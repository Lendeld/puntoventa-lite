using PuntoVenta.Application.DTOs.TarifasIvaImpuesto;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class TarifaIvaImpuestoMapper
{
    [MapperIgnoreTarget(nameof(TarifaIvaImpuestoDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(TarifaIvaImpuesto.FechaCreacion))]
    [MapperIgnoreSource(nameof(TarifaIvaImpuesto.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(TarifaIvaImpuesto.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(TarifaIvaImpuesto.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(TarifaIvaImpuesto.UsuarioModificacionId))]
    private static partial TarifaIvaImpuestoDto ToDtoGenerated(TarifaIvaImpuesto tarifaIvaImpuesto);

    public static TarifaIvaImpuestoDto ToDto(TarifaIvaImpuesto tarifaIvaImpuesto)
        => ToDtoGenerated(tarifaIvaImpuesto) with
        {
            ModificadoPor = tarifaIvaImpuesto.UsuarioModificacion?.Nombre,
        };
}
