using ErrorOr;

namespace PuntoVenta.Domain.Entities.TarifasIvaImpuesto;

public static class TarifaIvaImpuestoErrors
{
    public static Error CodigoRequerido =>
        Error.Validation("TarifaIvaImpuesto_Codigo", "El código es requerido.");

    public static Error CodigoLongitudInvalida =>
        Error.Validation("TarifaIvaImpuesto_Codigo", $"El código debe tener exactamente {TarifaIvaImpuesto.CodigoMaxLength} caracteres.");

    public static Error DetalleRequerido =>
        Error.Validation("TarifaIvaImpuesto_Detalle", "El detalle es requerido.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("TarifaIvaImpuesto_Detalle", $"El detalle no puede exceder {TarifaIvaImpuesto.DetalleMaxLength} caracteres.");

    public static Error PorcentajeInvalido =>
        Error.Validation("TarifaIvaImpuesto_Porcentaje", "El porcentaje debe estar entre 0 y 100.");

    public static Error ComentarioExcedeLongitud =>
        Error.Validation("TarifaIvaImpuesto_Comentario", $"El comentario no puede exceder {TarifaIvaImpuesto.ComentarioMaxLength} caracteres.");

    public static Error CodigoYaExiste =>
        Error.Conflict("TarifaIvaImpuesto_Codigo", "Ya existe una tarifa IVA con ese código.");

    public static Error NoEncontrado =>
        Error.NotFound("TarifaIvaImpuesto_NoEncontrado", "La tarifa IVA no existe.");
}
