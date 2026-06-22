using ErrorOr;

namespace PuntoVenta.Domain.Entities.Roles;

public static class RolErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Rol_Nombre", "El nombre del rol es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Rol_Nombre", $"El nombre no puede exceder {Rol.NombreMaxLength} caracteres.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("Rol_Descripcion", $"La descripción no puede exceder {Rol.DescripcionMaxLength} caracteres.");

    public static Error NombreYaExiste =>
        Error.Conflict("Rol_Nombre", "Ya existe un rol con ese nombre.");

    public static Error NoEncontrado =>
        Error.NotFound("Rol_NoEncontrado", "El rol no existe.");

    public static Error RolPrincipalNoPermiteCambiarEstado =>
        Error.Validation("Rol_Activo", "El rol principal no permite cambiar su estado activo.");
}
