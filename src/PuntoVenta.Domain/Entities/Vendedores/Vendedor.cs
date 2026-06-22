using ErrorOr;

namespace PuntoVenta.Domain.Entities.Vendedores;

public sealed class Vendedor : BaseAuditableEntity
{
    public const int NombreMaxLength = 150;

    private Vendedor() { }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public bool IsPrincipal { get; private set; }

    public static ErrorOr<Vendedor> Crear(string nombre, bool isPrincipal = false)
    {
        var errores = Validar(nombre);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Vendedor
        {
            Nombre = nombre.Trim(),
            NombreNormalizado = NormalizarNombre(nombre),
            IsPrincipal = isPrincipal
        };
    }

    public ErrorOr<Success> Actualizar(string nombre, bool isPrincipal = false)
    {
        var errores = Validar(nombre);

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        NombreNormalizado = NormalizarNombre(nombre);
        IsPrincipal = isPrincipal;

        return Result.Success;
    }

    public void MarcarComoPrincipal() => IsPrincipal = true;

    public void QuitarPrincipal() => IsPrincipal = false;

    public static string NormalizarNombre(string nombre) => nombre.Trim().ToUpperInvariant();

    private static List<Error> Validar(string nombre)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(VendedorErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(VendedorErrors.NombreExcedeLongitud);
        }

        return errores;
    }
}
