using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public static class NegocioErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Negocio_Nombre", "El nombre es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Negocio_Nombre", $"El nombre no puede exceder {Negocio.NombreMaxLength} caracteres.");

    public static Error NombreComercialExcedeLongitud =>
        Error.Validation("Negocio_NombreComercial", $"El nombre comercial no puede exceder {Negocio.NombreComercialMaxLength} caracteres.");

    public static Error DireccionExcedeLongitud =>
        Error.Validation("Negocio_Direccion", $"La dirección no puede exceder {Negocio.DireccionMaxLength} caracteres.");

    public static Error IdentificacionExcedeLongitud =>
        Error.Validation("Negocio_Identificacion", $"La identificación no puede exceder {Negocio.IdentificacionMaxLength} caracteres.");

    public static Error CorreoExcedeLongitud =>
        Error.Validation("Negocio_Correo", $"El correo no puede exceder {Negocio.CorreoMaxLength} caracteres.");

    public static Error TelefonoExcedeLongitud =>
        Error.Validation("Negocio_Telefono", $"El teléfono no puede exceder {Negocio.TelefonoMaxLength} caracteres.");

    public static Error TipoCambioPredeterminadoInvalido =>
        Error.Validation("Negocio_TipoCambioPredeterminado", "El tipo de cambio predeterminado debe ser mayor a 0.");

    public static Error NoEncontrado =>
        Error.NotFound("Negocio_NoEncontrado", "El negocio no existe.");

    public static Error TerminosVersionInvalida =>
        Error.Validation("Negocio_TerminosVersion", "La versión de los términos no es la vigente.");
}
