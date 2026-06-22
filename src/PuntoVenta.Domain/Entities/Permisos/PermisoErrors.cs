using ErrorOr;

namespace PuntoVenta.Domain.Entities.Permisos;

public static class PermisoErrors
{
    public static Error ClaveRequerida =>
        Error.Validation("Permiso_Clave", "La clave del permiso es requerida.");

    public static Error ClaveExcedeLongitud =>
        Error.Validation("Permiso_Clave", $"La clave no puede exceder {Permiso.ClaveMaxLength} caracteres.");

    public static Error ClaveYaExiste =>
        Error.Conflict("Permiso_Clave", "Ya existe un permiso con esa clave.");

    public static Error DescripcionRequerida =>
        Error.Validation("Permiso_Descripcion", "La descripción del permiso es requerida.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("Permiso_Descripcion", $"La descripción no puede exceder {Permiso.DescripcionMaxLength} caracteres.");

    public static Error ModuloRequerido =>
        Error.Validation("Permiso_Modulo", "El módulo es requerido.");

    public static Error ModuloExcedeLongitud =>
        Error.Validation("Permiso_Modulo", $"El módulo no puede exceder {Permiso.ModuloMaxLength} caracteres.");

    public static Error NoEncontrado =>
        Error.NotFound("Permiso_NoEncontrado", "El permiso no existe.");
}
