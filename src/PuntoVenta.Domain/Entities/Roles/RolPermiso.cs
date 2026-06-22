using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.Domain.Entities.Roles;

public sealed class RolPermiso
{
    private RolPermiso() { }

    public Guid RolId     { get; private set; }
    public Guid PermisoId { get; private set; }

    public Rol?     Rol     { get; private set; }
    public Permiso? Permiso { get; private set; }

    public static RolPermiso Crear(Guid rolId, Guid permisoId) =>
        new() { RolId = rolId, PermisoId = permisoId };
}
