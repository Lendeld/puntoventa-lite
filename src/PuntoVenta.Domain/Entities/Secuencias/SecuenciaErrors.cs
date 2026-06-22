using ErrorOr;

namespace PuntoVenta.Domain.Entities.Secuencias;

public static class SecuenciaErrors
{
    public static Error TipoDocumentoInvalido =>
        Error.Validation("Secuencia_TipoDocumento", "El tipo de documento indicado no es válido.");

    public static Error NoEncontrada =>
        Error.NotFound("Secuencia_NoEncontrada", "No se encontró la secuencia para el tipo de documento indicado.");
}
