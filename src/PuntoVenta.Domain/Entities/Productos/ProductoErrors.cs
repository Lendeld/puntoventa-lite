using ErrorOr;

namespace PuntoVenta.Domain.Entities.Productos;

public static class ProductoErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("Producto_Codigo", "El código del producto es requerido.");

    public static Error CodigoExcedeLongitud =>
        Error.Validation("Producto_Codigo", $"El código no puede exceder {Producto.CodigoMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("Producto_Codigo", "Ya existe un producto con ese código.");

    public static Error CodigoBarrasExcedeLongitud =>
        Error.Validation("Producto_CodigoBarras", $"El código de barras no puede exceder {Producto.CodigoBarrasMaxLength} caracteres.");

    public static Error CodigoBarrasYaExiste =>
        Error.Conflict("Producto_CodigoBarras", "Ya existe un producto con ese código de barras.");

    public static Error NombreRequerido =>
        Error.Validation("Producto_Nombre", "El nombre del producto es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Producto_Nombre", $"El nombre no puede exceder {Producto.NombreMaxLength} caracteres.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("Producto_Descripcion", $"La descripción no puede exceder {Producto.DescripcionMaxLength} caracteres.");

    public static Error ImagenUrlExcedeLongitud =>
        Error.Validation("Producto_ImagenUrl", $"La URL de imagen no puede exceder {Producto.ImagenUrlMaxLength} caracteres.");

    public static Error TipoItemInvalido =>
        Error.Validation("Producto_TipoItem", "El tipo de ítem no es válido. Use Bien=1 o Servicio=2.");

    public static Error PrecioUnitarioInvalido =>
        Error.Validation("Producto_PrecioUnitario", "El precio unitario no puede ser negativo.");

    public static Error PrecioCostoInvalido =>
        Error.Validation("Producto_PrecioCosto", "El precio de costo no puede ser negativo.");

    public static Error NoAplicaExistenciasSoloBien =>
        Error.Validation("Producto_NoAplicaExistencias", "La opción No aplica existencias solo puede activarse para productos tipo bien.");

    public static Error TarifaIvaRequerida =>
        Error.Validation("Producto_TarifaIvaImpuestoCodigo", "La tarifa de IVA es requerida.");

    public static Error NoEncontrado =>
        Error.NotFound("Producto_NoEncontrado", "El producto no existe.");

    public static Error StockInsuficiente(Guid id, string codigo, string descripcion, decimal disponible, decimal solicitada) =>
        Error.Validation(
            $"Producto_StockInsuficiente_{id}",
            $"Stock insuficiente para {codigo} - {descripcion}: disponible {disponible}, solicitado {solicitada}.",
            new Dictionary<string, object> { ["severity"] = "warning" });
}
