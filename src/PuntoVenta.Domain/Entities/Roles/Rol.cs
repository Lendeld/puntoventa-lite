using ErrorOr;

namespace PuntoVenta.Domain.Entities.Roles;

public sealed class Rol : BaseAuditableEntity
{
    public const int NombreMaxLength = 100;
    public const int DescripcionMaxLength = 500;

    private Rol() { }

    public string Nombre { get; private set; } = string.Empty;

    public string? Descripcion { get; private set; }

    public bool IsPrincipal { get; private set; }

    private readonly List<RolPermiso> _rolPermisos = [];
    public IReadOnlyCollection<RolPermiso> RolPermisos => _rolPermisos;

    public static ErrorOr<Rol> Crear(string nombre, string? descripcion = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(RolErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(RolErrors.NombreExcedeLongitud);
        }

        if (descripcion is not null && descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(RolErrors.DescripcionExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Rol
        {
            Nombre = nombre.Trim(),
            Descripcion = descripcion?.Trim(),
            IsPrincipal = false
        };
    }

    public ErrorOr<Success> Actualizar(string nombre, string? descripcion)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(RolErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(RolErrors.NombreExcedeLongitud);
        }

        if (descripcion is not null && descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(RolErrors.DescripcionExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();

        return Result.Success;
    }

    public void AsignarPermisos(IEnumerable<Guid> permisosIds)
    {
        _rolPermisos.Clear();
        _rolPermisos.AddRange(permisosIds
            .Distinct()
            .Select(permisoId => RolPermiso.Crear(Id, permisoId)));
    }

    public void MarcarComoPrincipal() => IsPrincipal = true;
}
