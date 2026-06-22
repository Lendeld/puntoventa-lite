using ErrorOr;

namespace PuntoVenta.Domain.Entities.Categorias;

public static class CategoriaErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Categoria_Nombre", "El nombre de la categoría es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Categoria_Nombre", $"El nombre no puede exceder {Categoria.NombreMaxLength} caracteres.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("Categoria_Descripcion", $"La descripción no puede exceder {Categoria.DescripcionMaxLength} caracteres.");

    public static Error NombreYaExiste =>
        Error.Conflict("Categoria_Nombre", "Ya existe una categoría con ese nombre.");

    public static Error NoEncontrado =>
        Error.NotFound("Categoria_NoEncontrado", "La categoría no existe.");
}
