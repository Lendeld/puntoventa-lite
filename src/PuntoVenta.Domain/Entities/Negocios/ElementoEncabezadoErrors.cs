using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public static class ElementoEncabezadoErrors
{
    public static Error TipoInvalido =>
        Error.Validation("ElementoEncabezado_Tipo", "El tipo de elemento del encabezado no es válido.");

    public static Error TextoLibreRequerido =>
        Error.Validation(
            "ElementoEncabezado_TextoLibre",
            "El texto del elemento es requerido.");

    public static Error TextoLibreExcedeLongitud =>
        Error.Validation(
            "ElementoEncabezado_TextoLibre",
            $"El texto del elemento no puede exceder {ElementoEncabezado.MaxTextoLibreLength} caracteres.");
}
