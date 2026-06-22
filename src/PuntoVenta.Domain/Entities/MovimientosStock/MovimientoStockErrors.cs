using ErrorOr;

namespace PuntoVenta.Domain.Entities.MovimientosStock;

public static class MovimientoStockErrors
{
    public static Error ProductoRequerido =>
        Error.Validation("MovimientoStock_ProductoId", "El producto es requerido.");

    public static Error DeltaCero =>
        Error.Validation("MovimientoStock_Delta", "El delta no puede ser cero.");

    public static Error RazonExcedeLongitud =>
        Error.Validation("MovimientoStock_Razon", $"La razón no puede exceder {MovimientoStock.RazonMaxLength} caracteres.");

    public static Error NoEncontrado =>
        Error.NotFound("MovimientoStock_NoEncontrado", "Movimiento de stock no encontrado.");

    public static Error ProductoNoEncontrado =>
        Error.NotFound("MovimientoStock_ProductoNoEncontrado", "Producto no encontrado para ajuste de stock.");
}
