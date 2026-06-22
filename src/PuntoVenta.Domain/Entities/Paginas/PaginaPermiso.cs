using PuntoVenta.Domain.Entities.Permisos;

namespace PuntoVenta.Domain.Entities.Paginas;

public sealed class PaginaPermiso
{
    private PaginaPermiso() { }

    public Guid PaginaId  { get; private set; }
    public Guid PermisoId { get; private set; }

    public Pagina?  Pagina  { get; private set; }
    public Permiso? Permiso { get; private set; }

    public static PaginaPermiso Crear(Guid paginaId, Guid permisoId) =>
        new() { PaginaId = paginaId, PermisoId = permisoId };
}
