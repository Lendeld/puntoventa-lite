using ErrorOr;

namespace PuntoVenta.Domain.Entities.MediosPago;

public static class MedioPagoErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("MedioPago_Codigo", "El código es requerido.");

    public static Error CodigoLongitudInvalida =>
        Error.Validation("MedioPago_Codigo", $"El código debe tener exactamente {MedioPago.CodigoMaxLength} caracteres.");

    public static Error DetalleRequerido =>
        Error.Validation("MedioPago_Detalle", "El detalle es requerido.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("MedioPago_Detalle", $"El detalle no puede exceder {MedioPago.DetalleMaxLength} caracteres.");

    public static Error ComentarioExcedeLongitud =>
        Error.Validation("MedioPago_Comentario", $"El comentario no puede exceder {MedioPago.ComentarioMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("MedioPago_Codigo", "Ya existe un medio de pago con ese código.");

    public static Error NoEncontrado =>
        Error.NotFound("MedioPago_NoEncontrado", "El medio de pago no existe.");
}
