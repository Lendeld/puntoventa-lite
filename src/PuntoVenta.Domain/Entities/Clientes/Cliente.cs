using ErrorOr;

namespace PuntoVenta.Domain.Entities.Clientes;

public sealed class Cliente : BaseAuditableEntity
{
    public const int NombreMaxLength = 100;
    public const int IdentificacionMaxLength = 20;
    public const int CorreoMaxLength = 160;
    public const int TelefonoMaxLength = 20;
    public const int ObservacionesMaxLength = 500;

    private Cliente() { }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public string? Identificacion { get; private set; }
    public string? Correo { get; private set; }
    public string? Telefono { get; private set; }
    public string? Observaciones { get; private set; }

    public static ErrorOr<Cliente> Crear(
        string nombre,
        string? identificacion = null,
        string? correo = null,
        string? telefono = null,
        string? observaciones = null)
    {
        var errores = Validar(nombre, identificacion, correo, telefono, observaciones);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Cliente
        {
            Nombre = nombre.Trim(),
            NombreNormalizado = NormalizarNombre(nombre),
            Identificacion = NormalizeNullable(identificacion),
            Correo = NormalizeNullable(correo),
            Telefono = NormalizeNullable(telefono),
            Observaciones = NormalizeNullable(observaciones)
        };
    }

    public ErrorOr<Success> Actualizar(
        string nombre,
        string? identificacion = null,
        string? correo = null,
        string? telefono = null,
        string? observaciones = null)
    {
        var errores = Validar(nombre, identificacion, correo, telefono, observaciones);

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        NombreNormalizado = NormalizarNombre(nombre);
        Identificacion = NormalizeNullable(identificacion);
        Correo = NormalizeNullable(correo);
        Telefono = NormalizeNullable(telefono);
        Observaciones = NormalizeNullable(observaciones);

        return Result.Success;
    }

    public static string NormalizarNombre(string nombre) => nombre.Trim().ToUpperInvariant();

    private static List<Error> Validar(
        string nombre,
        string? identificacion,
        string? correo,
        string? telefono,
        string? observaciones)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(ClienteErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(ClienteErrors.NombreExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(identificacion) && identificacion.Trim().Length > IdentificacionMaxLength)
        {
            errores.Add(ClienteErrors.IdentificacionExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(correo) && correo.Trim().Length > CorreoMaxLength)
        {
            errores.Add(ClienteErrors.CorreoExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(telefono) && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(ClienteErrors.TelefonoExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(observaciones) && observaciones.Trim().Length > ObservacionesMaxLength)
        {
            errores.Add(ClienteErrors.ObservacionesExcedeLongitud);
        }

        return errores;
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
