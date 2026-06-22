using PuntoVenta.Application.DTOs.MediosPago;
using PuntoVenta.Domain.Entities.MediosPago;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class MedioPagoMapper
{
    [MapperIgnoreTarget(nameof(MedioPagoDto.ModificadoPor))]
    [MapperIgnoreSource(nameof(MedioPago.FechaCreacion))]
    [MapperIgnoreSource(nameof(MedioPago.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(MedioPago.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(MedioPago.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(MedioPago.UsuarioModificacionId))]
    private static partial MedioPagoDto ToDtoGenerated(MedioPago medioPago);

    public static MedioPagoDto ToDto(MedioPago medioPago)
        => ToDtoGenerated(medioPago) with
        {
            ModificadoPor = medioPago.UsuarioModificacion?.Nombre,
        };
}
