using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public static class DocumentoVentaLineaErrors
{
    public static Error ProductoRequerido =>
        Error.Validation("DocumentoVentaLinea_ProductoId", "El producto es requerido.");

    public static Error CodigoRequerido =>
        Error.Validation("DocumentoVentaLinea_Codigo", "El código de la línea es requerido.");

    public static Error CodigoExcedeLongitud =>
        Error.Validation("DocumentoVentaLinea_Codigo", $"El código no puede exceder {DocumentoVentaLinea.CodigoMaxLength} caracteres.");

    public static Error DescripcionRequerida =>
        Error.Validation("DocumentoVentaLinea_Descripcion", "La descripción de la línea es requerida.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("DocumentoVentaLinea_Descripcion", $"La descripción no puede exceder {DocumentoVentaLinea.DescripcionMaxLength} caracteres.");

    public static Error UnidadMedidaExcedeLongitud =>
        Error.Validation("DocumentoVentaLinea_UnidadMedidaCodigo", $"La unidad de medida no puede exceder {DocumentoVentaLinea.UnidadMedidaCodigoMaxLength} caracteres.");

    public static Error CantidadInvalida =>
        Error.Validation("DocumentoVentaLinea_Cantidad", "La cantidad debe ser mayor a cero.");

    public static Error PrecioInvalido =>
        Error.Validation("DocumentoVentaLinea_PrecioUnitario", "El precio unitario no puede ser negativo.");

    public static Error DescuentoInvalido =>
        Error.Validation("DocumentoVentaLinea_MontoDescuento", "El descuento no puede ser negativo ni superar el monto bruto.");

    public static Error ImpuestoInvalido =>
        Error.Validation("DocumentoVentaLinea_MontoImpuesto", "El impuesto no puede ser negativo.");

    public static Error TipoItemInvalido =>
        Error.Validation("DocumentoVentaLinea_TipoItem", "El tipo de item es inválido.");
}
