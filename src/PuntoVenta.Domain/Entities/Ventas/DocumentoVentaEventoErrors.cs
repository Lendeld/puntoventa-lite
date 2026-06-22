using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public static class DocumentoVentaEventoErrors
{
    public static Error DocumentoRequerido =>
        Error.Validation("DocumentoVentaEvento_DocumentoVentaId", "El documento de venta del evento es requerido.");

    public static Error TipoRequerido =>
        Error.Validation("DocumentoVentaEvento_TipoEventoCodigo", "El tipo de evento es requerido.");

    public static Error TipoExcedeLongitud =>
        Error.Validation("DocumentoVentaEvento_TipoEventoCodigo", $"El código del tipo no puede exceder {DocumentoVentaEvento.TipoCodigoMaxLength} caracteres.");

    public static Error TipoNoExiste =>
        Error.NotFound("DocumentoVentaEvento_TipoEventoCodigo", "El tipo de evento indicado no existe en el catálogo.");

    public static Error ResumenRequerido =>
        Error.Validation("DocumentoVentaEvento_Resumen", "El resumen del evento es requerido.");

    public static Error ResumenExcedeLongitud =>
        Error.Validation("DocumentoVentaEvento_Resumen", $"El resumen no puede exceder {DocumentoVentaEvento.ResumenMaxLength} caracteres.");

    public static Error FechaRequerida =>
        Error.Validation("DocumentoVentaEvento_OcurridoEn", "La fecha de ocurrencia del evento es requerida.");
}

public static class TipoDocumentoVentaEventoErrors
{
    public static Error NoEncontrado =>
        Error.NotFound("TipoDocumentoVentaEvento_NoEncontrado", "El tipo de evento no existe.");

    public static Error CodigoRequerido =>
        Error.Validation("TipoDocumentoVentaEvento_Codigo", "El código del tipo de evento es requerido.");

    public static Error CodigoExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_Codigo", $"El código no puede exceder {TipoDocumentoVentaEvento.CodigoMaxLength} caracteres.");

    public static Error NombreRequerido =>
        Error.Validation("TipoDocumentoVentaEvento_Nombre", "El nombre del tipo de evento es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_Nombre", $"El nombre no puede exceder {TipoDocumentoVentaEvento.NombreMaxLength} caracteres.");

    public static Error CategoriaRequerida =>
        Error.Validation("TipoDocumentoVentaEvento_Categoria", "La categoría es requerida.");

    public static Error CategoriaExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_Categoria", $"La categoría no puede exceder {TipoDocumentoVentaEvento.CategoriaMaxLength} caracteres.");

    public static Error DescripcionExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_Descripcion", $"La descripción no puede exceder {TipoDocumentoVentaEvento.DescripcionMaxLength} caracteres.");

    public static Error IconoExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_IconoSugerido", $"El icono no puede exceder {TipoDocumentoVentaEvento.IconoMaxLength} caracteres.");

    public static Error ColorExcedeLongitud =>
        Error.Validation("TipoDocumentoVentaEvento_ColorSugerido", $"El color no puede exceder {TipoDocumentoVentaEvento.ColorMaxLength} caracteres.");
}
