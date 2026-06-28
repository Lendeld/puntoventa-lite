using System.Text.RegularExpressions;
using ErrorOr;

namespace PuntoVenta.Domain.Entities.Proveedores;

public sealed class Proveedor : BaseAuditableEntity
{
    public const int NombreMaxLength = 100;
    public const int CorreoMaxLength = 160;
    public const int TelefonoMaxLength = 20;
    public const int ObservacionMaxLength = 500;

    private static readonly Regex _regexCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private Proveedor() { }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public string? Correo { get; private set; }
    public string? Telefono { get; private set; }
    public string? Observacion { get; private set; }

    public static ErrorOr<Proveedor> Crear(
        string nombre,
        string? correo = null,
        string? telefono = null,
        string? observacion = null)
    {
        var errores = Validar(nombre, correo, telefono, observacion);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Proveedor
        {
            Nombre = nombre.Trim(),
            NombreNormalizado = NormalizarNombre(nombre),
            Correo = NormalizeNullable(correo),
            Telefono = NormalizeNullable(telefono),
            Observacion = NormalizeNullable(observacion)
        };
    }

    public ErrorOr<Success> Actualizar(
        string nombre,
        string? correo = null,
        string? telefono = null,
        string? observacion = null)
    {
        var errores = Validar(nombre, correo, telefono, observacion);

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        NombreNormalizado = NormalizarNombre(nombre);
        Correo = NormalizeNullable(correo);
        Telefono = NormalizeNullable(telefono);
        Observacion = NormalizeNullable(observacion);

        return Result.Success;
    }

    public static string NormalizarNombre(string nombre) => nombre.Trim().ToUpperInvariant();

    private static List<Error> Validar(
        string nombre,
        string? correo,
        string? telefono,
        string? observacion)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(ProveedorErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(ProveedorErrors.NombreExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(correo))
        {
            var correoTrimmed = correo.Trim();
            if (correoTrimmed.Length > CorreoMaxLength)
            {
                errores.Add(ProveedorErrors.CorreoExcedeLongitud);
            }
            else if (!_regexCorreo.IsMatch(correoTrimmed))
            {
                errores.Add(ProveedorErrors.CorreoInvalido);
            }
        }

        if (!string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(ProveedorErrors.TelefonoExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(observacion) && observacion.Trim().Length > ObservacionMaxLength)
        {
            errores.Add(ProveedorErrors.ObservacionExcedeLongitud);
        }

        return errores;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
