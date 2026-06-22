using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public sealed class Negocio : BaseAuditableEntity
{
    public const int NombreMaxLength = 100;
    public const int NombreComercialMaxLength = 80;
    public const int DireccionMaxLength = 255;
    public const int IdentificacionMaxLength = 20;
    public const int CorreoMaxLength = 160;
    public const int TelefonoMaxLength = 20;
    public const int LogoUrlMaxLength = 500;
    public const int TerminosVersionMaxLength = 40;
    public const decimal TipoCambioPredeterminadoDefault = 500m;

    private Negocio() { }

    public string Nombre { get; private set; } = string.Empty;

    public string? NombreComercial { get; private set; }

    public string? Direccion { get; private set; }

    public string? Identificacion { get; private set; }

    public string? Correo { get; private set; }

    public string? Telefono { get; private set; }

    public bool AplicaVendedores { get; private set; }

    public bool AplicaCajas { get; private set; }

    public string? LogoUrl { get; private set; }

    public decimal TipoCambioPredeterminado { get; private set; } = TipoCambioPredeterminadoDefault;

    public string? TerminosAceptadosVersion { get; private set; }

    public DateTime? TerminosAceptadosFechaUtc { get; private set; }

    public void AceptarTerminos(string version, DateTime fechaUtc)
    {
        TerminosAceptadosVersion = version.Trim();
        TerminosAceptadosFechaUtc = fechaUtc;
    }

    public static ErrorOr<Negocio> Crear(
        string nombre,
        string? nombreComercial = null,
        string? direccion = null,
        string? identificacion = null,
        string? correo = null,
        string? telefono = null,
        bool aplicaVendedores = false,
        bool aplicaGestionCajas = false,
        decimal? tipoCambioPredeterminado = null)
    {
        var errores = Validar(nombre, nombreComercial, direccion, identificacion, correo, telefono, tipoCambioPredeterminado);

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Negocio
        {
            Nombre = nombre.Trim(),
            NombreComercial = string.IsNullOrWhiteSpace(nombreComercial) ? null : nombreComercial.Trim(),
            Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim(),
            Identificacion = string.IsNullOrWhiteSpace(identificacion) ? null : identificacion.Trim(),
            Correo = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim(),
            Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim(),
            AplicaVendedores = aplicaVendedores,
            AplicaCajas = aplicaGestionCajas,
            TipoCambioPredeterminado = tipoCambioPredeterminado is > 0 ? tipoCambioPredeterminado.Value : TipoCambioPredeterminadoDefault
        };
    }

    public ErrorOr<Success> Actualizar(
        string nombre,
        string? nombreComercial = null,
        string? direccion = null,
        string? identificacion = null,
        string? correo = null,
        string? telefono = null,
        bool aplicaVendedores = false,
        bool aplicaGestionCajas = false,
        decimal? tipoCambioPredeterminado = null)
    {
        var errores = Validar(nombre, nombreComercial, direccion, identificacion, correo, telefono, tipoCambioPredeterminado);

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        NombreComercial = string.IsNullOrWhiteSpace(nombreComercial) ? null : nombreComercial.Trim();
        Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        Identificacion = string.IsNullOrWhiteSpace(identificacion) ? null : identificacion.Trim();
        Correo = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim();
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        AplicaVendedores = aplicaVendedores;
        AplicaCajas = aplicaGestionCajas;
        if (tipoCambioPredeterminado is > 0)
        {
            TipoCambioPredeterminado = tipoCambioPredeterminado.Value;
        }

        return Result.Success;
    }

    public void ActualizarLogo(string? logoUrl) => LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();

    private static List<Error> Validar(
        string nombre,
        string? nombreComercial,
        string? direccion,
        string? identificacion,
        string? correo,
        string? telefono,
        decimal? tipoCambioPredeterminado)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(NegocioErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(NegocioErrors.NombreExcedeLongitud);
        }

        if (nombreComercial is not null && nombreComercial.Trim().Length > NombreComercialMaxLength)
        {
            errores.Add(NegocioErrors.NombreComercialExcedeLongitud);
        }

        if (direccion is not null && direccion.Trim().Length > DireccionMaxLength)
        {
            errores.Add(NegocioErrors.DireccionExcedeLongitud);
        }

        if (identificacion is not null && identificacion.Trim().Length > IdentificacionMaxLength)
        {
            errores.Add(NegocioErrors.IdentificacionExcedeLongitud);
        }

        if (!string.IsNullOrWhiteSpace(correo) && correo.Trim().Length > CorreoMaxLength)
        {
            errores.Add(NegocioErrors.CorreoExcedeLongitud);
        }

        if (telefono is not null && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(NegocioErrors.TelefonoExcedeLongitud);
        }

        if (tipoCambioPredeterminado is <= 0)
        {
            errores.Add(NegocioErrors.TipoCambioPredeterminadoInvalido);
        }

        return errores;
    }
}
