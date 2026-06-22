using ErrorOr;

namespace PuntoVenta.Domain.Entities.Paginas;

public static class PaginaErrors
{
    public static Error NombreRequerido =>
        Error.Validation("Pagina_Nombre", "El nombre de la página es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Pagina_Nombre", $"El nombre no puede exceder {Pagina.NombreMaxLength} caracteres.");

    public static Error RutaRequerida =>
        Error.Validation("Pagina_Ruta", "La ruta de la página es requerida.");

    public static Error RutaExcedeLongitud =>
        Error.Validation("Pagina_Ruta", $"La ruta no puede exceder {Pagina.RutaMaxLength} caracteres.");

    public static Error IconoExcedeLongitud =>
        Error.Validation("Pagina_Icono", $"El ícono no puede exceder {Pagina.IconoMaxLength} caracteres.");

    public static Error NoEncontrada =>
        Error.NotFound("Pagina_NoEncontrada", "La página no existe.");
}
