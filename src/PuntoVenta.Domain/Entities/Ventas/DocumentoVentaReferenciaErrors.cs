using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public static class DocumentoVentaReferenciaErrors
{
    public static Error DocumentoReferenciaRequerido =>
        Error.Validation("DocumentoVentaReferencia_DocumentoReferenciaId", "El documento de referencia es requerido.");

    public static Error TipoDocRequerido =>
        Error.Validation("DocumentoVentaReferencia_TipoDocReferencia", "El tipo de documento de referencia es requerido.");

    public static Error TipoDocExcedeLongitud =>
        Error.Validation("DocumentoVentaReferencia_TipoDocReferencia", $"El tipo de documento de referencia no puede exceder {DocumentoVentaReferencia.TipoDocReferenciaMaxLength} caracteres.");

    public static Error RazonExcedeLongitud =>
        Error.Validation("DocumentoVentaReferencia_Razon", $"La razón de referencia no puede exceder {DocumentoVentaReferencia.RazonMaxLength} caracteres.");

    public static Error FechaRequerida =>
        Error.Validation("DocumentoVentaReferencia_FechaDocumentoReferencia", "La fecha del documento de referencia es requerida.");
}
