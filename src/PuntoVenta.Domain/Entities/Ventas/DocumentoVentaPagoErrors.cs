using ErrorOr;

namespace PuntoVenta.Domain.Entities.Ventas;

public static class DocumentoVentaPagoErrors
{
    public static Error MonedaRequerida =>
        Error.Validation("DocumentoVentaPago_MonedaCodigo", "La moneda del pago es requerida.");

    public static Error MonedaExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_MonedaCodigo", $"La moneda del pago no puede exceder {DocumentoVentaPago.MonedaCodigoMaxLength} caracteres.");

    public static Error MedioPagoRequerido =>
        Error.Validation("DocumentoVentaPago_MedioPagoCodigo", "El medio de pago es requerido.");

    public static Error MedioPagoExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_MedioPagoCodigo", $"El medio de pago no puede exceder {DocumentoVentaPago.MedioPagoCodigoMaxLength} caracteres.");

    public static Error DetalleExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_MedioPagoDetalleSnapshot", $"El detalle del medio de pago no puede exceder {DocumentoVentaPago.MedioPagoDetalleSnapshotMaxLength} caracteres.");

    public static Error TipoCambioInvalido =>
        Error.Validation("DocumentoVentaPago_TipoCambioAplicado", "El tipo de cambio aplicado debe ser mayor a cero.");

    public static Error MontoEntregadoInvalido =>
        Error.Validation("DocumentoVentaPago_MontoEntregado", "El monto entregado debe ser mayor a cero.");

    public static Error MontoAplicadoMonedaPagoInvalido =>
        Error.Validation("DocumentoVentaPago_MontoAplicadoMonedaPago", "El monto aplicado en la moneda del pago debe ser mayor a cero.");

    public static Error MontoAplicadoDocumentoInvalido =>
        Error.Validation("DocumentoVentaPago_MontoAplicadoDocumento", "El monto aplicado en la moneda del documento debe ser mayor a cero.");

    public static Error MontoVueltoMonedaPagoInvalido =>
        Error.Validation("DocumentoVentaPago_MontoVueltoMonedaPago", "El vuelto en la moneda del pago no puede ser negativo.");

    public static Error MontoVueltoDocumentoInvalido =>
        Error.Validation("DocumentoVentaPago_MontoVueltoDocumento", "El vuelto en la moneda del documento no puede ser negativo.");

    public static Error MontoEntregadoNoCuadra =>
        Error.Validation("DocumentoVentaPago_MontoEntregado", "El monto entregado debe ser igual al monto aplicado más el vuelto.");

    public static Error ReferenciaExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_Referencia", $"La referencia no puede exceder {DocumentoVentaPago.ReferenciaMaxLength} caracteres.");

    public static Error ObservacionExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_Observacion", $"La observación no puede exceder {DocumentoVentaPago.ObservacionMaxLength} caracteres.");

    public static Error YaAnulado =>
        Error.Conflict(
            "DocumentoVentaPago_YaAnulado",
            "El abono ya fue anulado previamente.",
            new Dictionary<string, object> { ["severity"] = "warning" });

    public static Error MotivoAnulacionRequerido =>
        Error.Validation("DocumentoVentaPago_MotivoAnulacion", "El motivo de anulación es requerido.");

    public static Error MotivoAnulacionExcedeLongitud =>
        Error.Validation("DocumentoVentaPago_MotivoAnulacion", $"El motivo de anulación no puede exceder {DocumentoVentaPago.MotivoAnulacionMaxLength} caracteres.");

    public static Error UsuarioAnulaInvalido =>
        Error.Validation("DocumentoVentaPago_UsuarioAnulaId", "El usuario que anula el abono es inválido.");

    public static Error NoEncontrado =>
        Error.NotFound("DocumentoVentaPago_NoEncontrado", "El abono indicado no existe en este documento.");
}
