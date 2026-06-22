using ErrorOr;

namespace PuntoVenta.Domain.Entities.Clientes;

public static class ClienteErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Cliente_Nombre", "El nombre es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Cliente_Nombre", $"El nombre no puede exceder {Cliente.NombreMaxLength} caracteres.");

    public static Error IdentificacionExcedeLongitud =>
        Error.Validation("Cliente_Identificacion", $"La identificación no puede exceder {Cliente.IdentificacionMaxLength} caracteres.");

    public static Error CorreoExcedeLongitud =>
        Error.Validation("Cliente_Correo", $"El correo no puede exceder {Cliente.CorreoMaxLength} caracteres.");

    public static Error TelefonoExcedeLongitud =>
        Error.Validation("Cliente_Telefono", $"El teléfono no puede exceder {Cliente.TelefonoMaxLength} caracteres.");

    public static Error ObservacionesExcedeLongitud =>
        Error.Validation("Cliente_Observaciones", $"Las observaciones no pueden exceder {Cliente.ObservacionesMaxLength} caracteres.");

    public static Error IdentificacionYaExiste =>
        Error.Conflict("Cliente_Identificacion_YaExiste", "Ya existe un cliente con esa identificación.");

    public static Error NoEncontrado =>
        Error.NotFound("Cliente_NoEncontrado", "El cliente no existe.");
}
