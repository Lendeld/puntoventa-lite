using PuntoVenta.Application.DTOs.Negocios;
using PuntoVenta.Domain.Entities.Negocios;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class NegocioTicketConfigMapper
{
    [MapperIgnoreSource(nameof(NegocioTicketConfig.FechaCreacion))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.FechaModificacion))]
    [MapperIgnoreSource(nameof(NegocioTicketConfig.Activo))]
    public static partial NegocioTicketConfigDto ToDto(NegocioTicketConfig entity);

    [MapperIgnoreSource(nameof(ConfiguracionPieDocumento.EsTodos))]
    private static partial ConfiguracionPieDocumentoDto ToDto(ConfiguracionPieDocumento configuracion);

    private static partial LineaPieDocumentoDto ToDto(LineaPieDocumento linea);

    [MapperIgnoreSource(nameof(ElementoEncabezado.EsFijo))]
    private static partial ElementoEncabezadoDto ToDto(ElementoEncabezado elemento);
}
