using ErrorOr;

namespace PuntoVenta.Domain.Entities.Cajas;

public sealed class Caja : BaseAuditableEntity
{
    public const int CodigoMaxLength = 8;
    public const int NombreMaxLength = 100;

    private Caja() { }

    public string Codigo { get; private set; } = string.Empty;
    public string CodigoNormalizado { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;

    public static ErrorOr<Caja> Crear(
        string codigo,
        string nombre)
    {
        var errores = Validar(codigo, nombre);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Caja
        {
            Codigo = codigo.Trim(),
            CodigoNormalizado = NormalizarCodigo(codigo),
            Nombre = nombre.Trim()
        };
    }

    public ErrorOr<Success> Actualizar(string codigo, string nombre)
    {
        var errores = Validar(codigo, nombre);

        if (errores.Count > 0)
        {
            return errores;
        }

        Codigo = codigo.Trim();
        CodigoNormalizado = NormalizarCodigo(codigo);
        Nombre = nombre.Trim();

        return Result.Success;
    }

    public static string NormalizarCodigo(string codigo) => codigo.Trim().ToUpperInvariant();

    private static List<Error> Validar(string codigo, string nombre)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add(CajaErrors.CodigoRequerido);
        }
        else if (codigo.Trim().Length > CodigoMaxLength)
        {
            errores.Add(CajaErrors.CodigoExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(CajaErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(CajaErrors.NombreExcedeLongitud);
        }

        return errores;
    }
}
