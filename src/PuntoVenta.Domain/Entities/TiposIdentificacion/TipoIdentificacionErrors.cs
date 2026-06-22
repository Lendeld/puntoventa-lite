using ErrorOr;

namespace PuntoVenta.Domain.Entities.TiposIdentificacion;

public static class TipoIdentificacionErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("TipoIdentificacion_Codigo", "El código es requerido.");

    public static Error CodigoLongitudInvalida =>
        Error.Validation("TipoIdentificacion_Codigo", $"El código debe tener exactamente {TipoIdentificacion.CodigoMaxLength} caracteres.");

    public static Error DetalleRequerido =>
        Error.Validation("TipoIdentificacion_Detalle", "El detalle es requerido.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("TipoIdentificacion_Detalle", $"El detalle no puede exceder {TipoIdentificacion.DetalleMaxLength} caracteres.");

    public static Error ComentarioExcedeLongitud =>
        Error.Validation("TipoIdentificacion_Comentario", $"El comentario no puede exceder {TipoIdentificacion.ComentarioMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("TipoIdentificacion_Codigo", "Ya existe un tipo de identificación con ese código.");

    public static Error NoEncontrado =>
        Error.NotFound("TipoIdentificacion_NoEncontrado", "El tipo de identificación no existe.");

    public static Error Inactivo =>
        Error.Validation("TipoIdentificacion_Activo", "El tipo de identificación está inactivo.");
}
