using ErrorOr;

namespace PuntoVenta.Domain.Entities.Categorias;

public sealed class Categoria : BaseAuditableEntity
{
    public const int NombreMaxLength = 150;
    public const int DescripcionMaxLength = 255;

    private Categoria() { }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    public static ErrorOr<Categoria> Crear(string nombre, string? descripcion = null)
    {
        var errores = Validar(nombre, descripcion);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Categoria
        {
            Nombre = nombre.Trim(),
            NombreNormalizado = NormalizarNombre(nombre),
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()
        };
    }

    public ErrorOr<Success> Actualizar(string nombre, string? descripcion = null)
    {
        var errores = Validar(nombre, descripcion);

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        NombreNormalizado = NormalizarNombre(nombre);
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        return Result.Success;
    }

    public static string NormalizarNombre(string nombre) => nombre.Trim().ToUpperInvariant();

    private static List<Error> Validar(string nombre, string? descripcion)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(CategoriaErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(CategoriaErrors.NombreExcedeLongitud);
        }

        if (descripcion is not null && descripcion.Trim().Length > DescripcionMaxLength)
        {
            errores.Add(CategoriaErrors.DescripcionExcedeLongitud);
        }

        return errores;
    }
}
