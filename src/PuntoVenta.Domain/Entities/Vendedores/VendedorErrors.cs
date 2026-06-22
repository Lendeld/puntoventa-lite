using ErrorOr;

namespace PuntoVenta.Domain.Entities.Vendedores;

public static class VendedorErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Vendedor_Nombre", "El nombre del vendedor es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Vendedor_Nombre", $"El nombre no puede exceder {Vendedor.NombreMaxLength} caracteres.");

    public static Error NombreYaExiste =>
        Error.Conflict("Vendedor_Nombre", "Ya existe un vendedor con ese nombre.");

    public static Error PrincipalNoSePuedeDesactivar =>
        Error.Validation("Vendedor_Activo", "El vendedor principal no se puede desactivar.");

    public static Error PrincipalNoSePuedeQuitarSinReemplazo =>
        Error.Validation("Vendedor_IsPrincipal", "El vendedor principal no puede quedar sin reemplazo.");

    public static Error NoEncontrado =>
        Error.NotFound("Vendedor_NoEncontrado", "El vendedor no existe.");
}
