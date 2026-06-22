using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public static class DocumentoVentaErrors
{
    public static Error NoEncontrado =>
        Error.NotFound("DocumentoVenta_NoEncontrado", "El documento de venta no existe.");

    public static Error DocumentoOrigenNoEncontrado =>
        Error.NotFound("DocumentoVenta_OrigenNoEncontrado", "El documento de venta origen no existe.");

    public static Error PagoNoEncontrado =>
        Error.NotFound("DocumentoVentaPago_NoEncontrado", "El pago referenciado no existe en el documento.");

    public static Error NegocioRequerido =>
        Error.Validation("DocumentoVenta_NegocioId", "El negocio del documento es requerido.");

    public static Error TipoInvalido =>
        Error.Validation("DocumentoVenta_Tipo", "El tipo de documento de venta es inválido.");

    public static Error EstadoInvalido =>
        Error.Validation("DocumentoVenta_Estado", "El estado del documento de venta es inválido.");

    public static Error FechaRequerida =>
        Error.Validation("DocumentoVenta_FechaDocumento", "La fecha del documento es requerida.");

    public static Error FechaVencimientoRequerida =>
        Error.Validation("DocumentoVenta_FechaVencimiento", "La fecha de vencimiento del apartado es requerida.");

    public static Error FechaVencimientoInvalida =>
        Error.Validation("DocumentoVenta_FechaVencimiento", "La fecha de vencimiento debe ser posterior o igual a la fecha del documento.");

    public static Error CondicionVentaRequerida =>
        Error.Validation("DocumentoVenta_CondicionVentaCodigo", "La condición de venta es requerida.");

    public static Error CondicionVentaExcedeLongitud =>
        Error.Validation("DocumentoVenta_CondicionVentaCodigo", $"La condición de venta no puede exceder {DocumentoVenta.CondicionVentaCodigoMaxLength} caracteres.");

    public static Error MonedaRequerida =>
        Error.Validation("DocumentoVenta_MonedaCodigo", "La moneda es requerida.");

    public static Error MonedaExcedeLongitud =>
        Error.Validation("DocumentoVenta_MonedaCodigo", $"La moneda no puede exceder {DocumentoVenta.MonedaCodigoMaxLength} caracteres.");

    public static Error TipoCambioInvalido =>
        Error.Validation("DocumentoVenta_TipoCambio", "El tipo de cambio debe ser mayor a cero.");

    public static Error PlazoCreditoInvalido =>
        Error.Validation("DocumentoVenta_PlazoCreditoDias", "El plazo de crédito debe ser mayor a cero y expresado en días.");

    public static Error ConsecutivoRequerido =>
        Error.Validation("DocumentoVenta_Consecutivo", "El consecutivo del documento es requerido.");

    public static Error ConsecutivoYaAsignado =>
        Error.Conflict("DocumentoVenta_Consecutivo", "El documento ya tiene un consecutivo asignado.");

    public static Error DocumentoNoEditable =>
        Error.Conflict("DocumentoVenta_Estado", "El documento ya no se puede editar.");

    public static Error DocumentoNoEmitible =>
        Error.Conflict("DocumentoVenta_Estado", "El documento no se puede emitir en su estado actual.");

    public static Error DocumentoNoConvertible =>
        Error.Conflict("DocumentoVenta_Estado", "El apartado no se puede convertir en su estado actual.");

    public static Error DocumentoNoCancelable =>
        Error.Conflict("DocumentoVenta_Estado", "El apartado no se puede cancelar en su estado actual.");

    public static Error DetallesRequeridos =>
        Error.Validation("DocumentoVenta_Lineas", "El documento debe tener al menos una línea.");

    public static Error PagosRequeridos =>
        Error.Validation("DocumentoVenta_Pagos", "El documento requiere al menos un medio de pago.");

    public static Error PagosNoCuadran =>
        Error.Validation("DocumentoVenta_Pagos", "La suma de los medios de pago no coincide con el monto requerido.");

    public static Error PagoExcedeSaldo =>
        Error.Validation("DocumentoVenta_Pagos", "El pago no puede exceder el saldo pendiente del apartado.");

    public static Error ApartadoConSaldoPendiente =>
        Error.Conflict("DocumentoVenta_SaldoPendiente", "El apartado debe estar completamente pagado antes de convertirse en factura.");

    public static Error PagosExcedenMaximo =>
        Error.Validation("DocumentoVenta_Pagos", "La factura no puede tener más de 4 medios de pago.");

    public static Error ReferenciaRequerida =>
        Error.Validation("DocumentoVenta_Referencia", "Las notas requieren un documento de referencia.");

    public static Error DocumentoOrigenNoEmitido =>
        Error.Conflict("DocumentoVenta_Referencia", "El documento origen debe estar emitido.");

    public static Error CreditoRequiereCliente =>
        Error.Validation(
            "DocumentoVenta_ClienteId",
            "Las ventas a crédito requieren un cliente identificado.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error AbonoSoloEnCredito =>
        Error.Conflict(
            "DocumentoVenta_Estado",
            "Solo se pueden registrar abonos sobre facturas de crédito.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error FechaPagoFutura =>
        Error.Validation("DocumentoVentaPago_FechaPago", "La fecha del pago no puede ser futura.");

    public static Error FechaPagoAnteriorAlDocumento =>
        Error.Validation("DocumentoVentaPago_FechaPago", "La fecha del pago no puede ser anterior a la fecha del documento.");

    public static Error VendedorNoEncontrado =>
        Error.Validation("DocumentoVenta_VendedorId", "El vendedor indicado no existe en el negocio activo.");

    public static Error AbonoSinSaldoPendiente =>
        Error.Conflict(
            "DocumentoVenta_SaldoPendiente",
            "El documento no tiene saldo pendiente.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error CajaRequerida =>
        Error.Validation("DocumentoVenta_CajaId", "La caja emisora es requerida.");

    public static Error NotaCreditoTipoOrigenInvalido =>
        Error.Validation(
            "DocumentoVenta_TipoOrigen",
            "Solo se permite emitir nota de crédito contra factura o nota de débito.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaCreditoLineasRequeridas =>
        Error.Validation(
            "DocumentoVenta_Lineas",
            "La nota de crédito debe tener al menos una línea.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaCreditoCorrigeMontoTotalReversa =>
        Error.Conflict(
            "DocumentoVenta_NotaCorrigeMontoTotalReversa",
            "Para reversar el total del documento usa modo Anulación (código razón 01). 'Corrige monto' es solo para ajuste parcial de precio.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaCreditoExcedeMontoOrigen(decimal acumulado, decimal totalOrigen) =>
        Error.Conflict(
            "DocumentoVenta_NotaExcedeMontoOrigen",
            $"El monto acumulado de notas de crédito ({acumulado:N2}) supera el total del documento origen ({totalOrigen:N2}).",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaCreditoSobreFacturaConAbonosActivos =>
        Error.Conflict(
            "DocumentoVenta_NotaCreditoSobreFacturaConAbonosActivos",
            "La factura tiene abonos activos; anulá los abonos antes de emitir una nota de crédito.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaDebitoTipoOrigenInvalido =>
        Error.Validation(
            "DocumentoVenta_TipoOrigen",
            "Solo se permite emitir nota de débito contra factura o tiquete.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaDebitoLineasRequeridas =>
        Error.Validation(
            "DocumentoVenta_Lineas",
            "La nota de débito debe tener al menos una línea.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error NotaDebitoSobreFacturaCredito =>
        Error.Conflict(
            "DocumentoVenta_NotaDebitoSobreFacturaCredito",
            "No se puede emitir una nota de débito sobre una factura a crédito. Las notas de débito solo aplican a facturas de contado.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error AnulacionConNotasDebitoVigentes(string consecutivos) =>
        Error.Conflict(
            "DocumentoVenta_AnulacionConNotasDebitoVigentes",
            $"El documento tiene notas de débito vigentes ({consecutivos}). Emite una nota de crédito contra cada una antes de anular el documento origen.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error SinCajaLocalHost =>
        Error.Conflict(
            "DocumentoVenta_SinCajaLocalHost",
            "No hay caja pareada. Termina el pareo con la nube antes de emitir documentos.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error SinCajaCloud =>
        Error.Conflict(
            "DocumentoVenta_SinCajaCloud",
            "El negocio no tiene caja principal configurada.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error AbonoFacturaCubiertaPorNotaCredito =>
        Error.Conflict(
            "DocumentoVenta_AbonoFacturaCubiertaPorNotaCredito",
            "La factura está cubierta o anulada por una nota de crédito; no admite abonos.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error DocumentoNoEmiteReciboAbono =>
        Error.Validation(
            "DocumentoVenta_Tipo",
            "Este documento no emite recibo de abono.",
            new Dictionary<string, object> { ["severity"] = "warning" });
}
