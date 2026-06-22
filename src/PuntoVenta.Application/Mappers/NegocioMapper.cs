using PuntoVenta.Application.DTOs.Negocios;
using PuntoVenta.Domain.Entities.Negocios;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class NegocioMapper
{
    [MapperIgnoreTarget(nameof(NegocioDto.ModificadoPor))]
    // TipoIdentificacionCodigo no existe en la entidad (Lite podó factura electrónica/CABYS);
    // el DTO lo conserva por compatibilidad de UI y queda en vacío.
    [MapperIgnoreTarget(nameof(NegocioDto.TipoIdentificacionCodigo))]
    [MapperIgnoreSource(nameof(Negocio.FechaCreacion))]
    [MapperIgnoreSource(nameof(Negocio.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(Negocio.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(Negocio.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(Negocio.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(Negocio.TerminosAceptadosVersion))]
    [MapperIgnoreSource(nameof(Negocio.TerminosAceptadosFechaUtc))]
    private static partial NegocioDto ToDtoGenerated(Negocio negocio);

    public static NegocioDto ToDto(Negocio negocio) => ToDtoGenerated(negocio) with
    {
        ModificadoPor = negocio.UsuarioModificacion?.Nombre,
    };

    // El negocio placeholder se siembra sin identificación/tipo, así que esos
    // campos (string? en la entidad) llegan null. Mapperly toma este método
    // para mapear string? -> string del DTO, coalesciendo null a vacío en vez
    // de lanzar ArgumentNullException.
    private static string NullableAVacio(string? value) => value ?? string.Empty;
}
