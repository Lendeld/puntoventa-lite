using ErrorOr;
using PuntoVenta.Domain.Entities.Roles;

namespace PuntoVenta.Domain.Entities.Permisos;

public sealed class Permiso : BaseAuditableEntity
{
    public const int ClaveMaxLength       = 100;
    public const int DescripcionMaxLength = 500;
    public const int ModuloMaxLength      = 100;

    private Permiso() { }

    public string Clave       { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Modulo      { get; private set; } = string.Empty;

    private readonly List<RolPermiso> _rolPermisos = [];
    public IReadOnlyCollection<RolPermiso> RolPermisos => _rolPermisos;

    public static ErrorOr<Permiso> Crear(string clave, string descripcion, string modulo)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(clave))
        {
            errores.Add(PermisoErrors.ClaveRequerida);
        }
        else if (clave.Trim().Length > ClaveMaxLength)
        {
            errores.Add(PermisoErrors.ClaveExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            errores.Add(PermisoErrors.DescripcionRequerida);
        }
        else if (descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(PermisoErrors.DescripcionExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(modulo))
        {
            errores.Add(PermisoErrors.ModuloRequerido);
        }
        else if (modulo.Trim().Length > ModuloMaxLength)
        {
            errores.Add(PermisoErrors.ModuloExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Permiso
        {
            Clave       = clave.Trim().ToLowerInvariant(),
            Descripcion = descripcion.Trim(),
            Modulo      = modulo.Trim().ToLowerInvariant()
        };
    }
}
