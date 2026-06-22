using ErrorOr;

namespace PuntoVenta.Domain.Entities.Cajas;

public static class CajaErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("Caja_Codigo", "El código de la caja es requerido.");

    public static Error CodigoExcedeLongitud =>
        Error.Validation("Caja_Codigo", $"El código no puede exceder {Caja.CodigoMaxLength} caracteres.");

    public static Error NombreRequerido =>
        Error.Validation("Caja_Nombre", "El nombre de la caja es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Caja_Nombre", $"El nombre no puede exceder {Caja.NombreMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("Caja_Codigo", "Ya existe una caja con ese código.");

    public static Error NoEncontrada =>
        Error.NotFound("Caja_NoEncontrada", "La caja no existe.");
}
