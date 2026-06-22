using PuntoVenta.Application.DTOs.CodigosImpuesto;
using PuntoVenta.Domain.Entities.CodigosImpuesto;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class CodigoImpuestoMapper
{
    [MapperIgnoreTarget(nameof(CodigoImpuestoDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(CodigoImpuesto.FechaCreacion))]
    [MapperIgnoreSource(nameof(CodigoImpuesto.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(CodigoImpuesto.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(CodigoImpuesto.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(CodigoImpuesto.UsuarioModificacionId))]
    private static partial CodigoImpuestoDto ToDtoGenerated(CodigoImpuesto codigoImpuesto);

    public static CodigoImpuestoDto ToDto(CodigoImpuesto codigoImpuesto)
        => ToDtoGenerated(codigoImpuesto) with
        {
            ModificadoPor = codigoImpuesto.UsuarioModificacion?.Nombre,
        };
}
