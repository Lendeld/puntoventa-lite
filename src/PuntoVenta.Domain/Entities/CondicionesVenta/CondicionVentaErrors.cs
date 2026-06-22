using ErrorOr;

namespace PuntoVenta.Domain.Entities.CondicionesVenta;

public static class CondicionVentaErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("CondicionVenta_Codigo", "El código es requerido.");

    public static Error CodigoLongitudInvalida =>
        Error.Validation("CondicionVenta_Codigo", $"El código debe tener exactamente {CondicionVenta.CodigoMaxLength} caracteres.");

    public static Error DetalleRequerido =>
        Error.Validation("CondicionVenta_Detalle", "El detalle es requerido.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("CondicionVenta_Detalle", $"El detalle no puede exceder {CondicionVenta.DetalleMaxLength} caracteres.");

    public static Error ComentarioExcedeLongitud =>
        Error.Validation("CondicionVenta_Comentario", $"El comentario no puede exceder {CondicionVenta.ComentarioMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("CondicionVenta_Codigo", "Ya existe una condición de venta con ese código.");

    public static Error NoEncontrado =>
        Error.NotFound("CondicionVenta_NoEncontrado", "La condición de venta no existe.");
}
