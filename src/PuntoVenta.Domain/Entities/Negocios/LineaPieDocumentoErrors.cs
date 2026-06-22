using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public static class LineaPieDocumentoErrors
{
    public static Error TextoRequerido =>
        Error.Validation("LineaPieDocumento_Texto", "El texto de la línea es requerido.");

    public static Error TextoExcedeLongitud =>
        Error.Validation(
            "LineaPieDocumento_Texto",
            $"El texto de la línea no puede exceder {LineaPieDocumento.MaxTextoLength} caracteres.");
}
