using PuntoVenta.Application.DTOs.Clientes;
using PuntoVenta.Domain.Entities.Clientes;

namespace PuntoVenta.Application.Mappers;

public static class ClienteMapper
{
    public static ClienteListaDto ToListaDto(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nombre = cliente.Nombre,
        Identificacion = cliente.Identificacion,
        Correo = cliente.Correo,
        Telefono = cliente.Telefono,
        Activo = cliente.Activo,
        FechaCreacion = cliente.FechaCreacion,
        FechaModificacion = cliente.FechaModificacion
    };

    public static ClienteDto ToDto(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nombre = cliente.Nombre,
        Identificacion = cliente.Identificacion,
        Correo = cliente.Correo,
        Telefono = cliente.Telefono,
        Observaciones = cliente.Observaciones,
        Activo = cliente.Activo,
        FechaCreacion = cliente.FechaCreacion,
        FechaModificacion = cliente.FechaModificacion
    };
}
