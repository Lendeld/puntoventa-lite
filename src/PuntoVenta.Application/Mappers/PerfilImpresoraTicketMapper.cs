using PuntoVenta.Application.DTOs.Impresion;
using PuntoVenta.Domain.Entities.Impresion;
using Riok.Mapperly.Abstractions;

namespace PuntoVenta.Application.Mappers;

[Mapper]
public static partial class PerfilImpresoraTicketMapper
{
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.FechaCreacion))]
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.UsuarioCreacionId))]
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.UsuarioCreacion))]
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.UsuarioModificacionId))]
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.UsuarioModificacion))]
    [MapperIgnoreSource(nameof(PerfilImpresoraTicket.FechaModificacion))]
    public static partial PerfilImpresoraTicketDto ToDto(PerfilImpresoraTicket entity);
}
