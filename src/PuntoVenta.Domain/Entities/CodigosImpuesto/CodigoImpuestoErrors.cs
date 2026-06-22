using ErrorOr;

namespace PuntoVenta.Domain.Entities.CodigosImpuesto;

public static class CodigoImpuestoErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("CodigoImpuesto_Codigo", "El código es requerido.");

    public static Error CodigoLongitudInvalida =>
        Error.Validation("CodigoImpuesto_Codigo", $"El código debe tener exactamente {CodigoImpuesto.CodigoMaxLength} caracteres.");

    public static Error DetalleRequerido =>
        Error.Validation("CodigoImpuesto_Detalle", "El detalle es requerido.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("CodigoImpuesto_Detalle", $"El detalle no puede exceder {CodigoImpuesto.DetalleMaxLength} caracteres.");

    public static Error ComentarioExcedeLongitud =>
        Error.Validation("CodigoImpuesto_Comentario", $"El comentario no puede exceder {CodigoImpuesto.ComentarioMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("CodigoImpuesto_Codigo", "Ya existe un código de impuesto con ese código.");

    public static Error NoEncontrado =>
        Error.NotFound("CodigoImpuesto_NoEncontrado", "El código de impuesto no existe.");
}
