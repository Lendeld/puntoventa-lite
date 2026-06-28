using ErrorOr;

namespace PuntoVenta.Domain.Entities.Proveedores;

public static class ProveedorErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Proveedor_Nombre", "El nombre del proveedor es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Proveedor_Nombre", $"El nombre no puede exceder {Proveedor.NombreMaxLength} caracteres.");

    public static Error CorreoExcedeLongitud =>
        Error.Validation("Proveedor_Correo", $"El correo no puede exceder {Proveedor.CorreoMaxLength} caracteres.");

    public static Error CorreoInvalido =>
        Error.Validation("Proveedor_Correo", "El correo no tiene un formato válido.");

    public static Error TelefonoExcedeLongitud =>
        Error.Validation("Proveedor_Telefono", $"El teléfono no puede exceder {Proveedor.TelefonoMaxLength} caracteres.");

    public static Error ObservacionExcedeLongitud =>
        Error.Validation("Proveedor_Observacion", $"La observación no puede exceder {Proveedor.ObservacionMaxLength} caracteres.");

    public static Error NombreYaExiste =>
        Error.Conflict("Proveedor_Nombre", "Ya existe un proveedor con ese nombre.");

    public static Error NoEncontrado =>
        Error.NotFound("Proveedor_NoEncontrado", "El proveedor no existe.");
}
