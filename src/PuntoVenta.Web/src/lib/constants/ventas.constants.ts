export const VENTA_FIELDS = {
    CLIENTE_ID: "DocumentoVenta_ClienteId",
    VENDEDOR_ID: "DocumentoVenta_VendedorId",
    CAJA_ID: "DocumentoVenta_CajaId",
    CONDICION_VENTA_CODIGO: "DocumentoVenta_CondicionVentaCodigo",
    FECHA_DOCUMENTO: "DocumentoVenta_FechaDocumento",
    FECHA_VENCIMIENTO: "DocumentoVenta_FechaVencimiento",
    MONEDA_CODIGO: "DocumentoVenta_MonedaCodigo",
    TIPO_CAMBIO: "DocumentoVenta_TipoCambio",
    PLAZO_CREDITO_DIAS: "DocumentoVenta_PlazoCreditoDias",
    OBSERVACIONES: "DocumentoVenta_Observaciones",
    LINEAS: "DocumentoVenta_Lineas",
    PAGOS: "DocumentoVenta_Pagos",
} as const;

export const CONDICIONES_VENTA_CREDITO = ["02", "10"] as const;
const CONDICIONES_VENTA_CREDITO_SET: ReadonlySet<string> = new Set(CONDICIONES_VENTA_CREDITO);

export function esCondicionVentaCredito(codigo: string): boolean {
    return CONDICIONES_VENTA_CREDITO_SET.has(codigo);
}
export const CONDICION_VENTA_CONTADO = "01" as const;

// Condiciones que NO se eligen manualmente en una factura de POS:
// 04 Apartado es un proceso (botón "Crear apartado"); 09 y 11 son de uso
// exclusivo de Recibos Electrónicos de Pago (REP), no de factura. Se ocultan
// del dropdown sin tocar el catálogo de Configuración.
export const CONDICIONES_VENTA_OCULTAS_FACTURACION = ["04", "09", "11"] as const;
const CONDICIONES_VENTA_OCULTAS_SET: ReadonlySet<string> = new Set(
    CONDICIONES_VENTA_OCULTAS_FACTURACION,
);

export function esCondicionVentaOcultaEnFacturacion(codigo: string): boolean {
    return CONDICIONES_VENTA_OCULTAS_SET.has(codigo);
}

export const MEDIO_PAGO_EFECTIVO = "01" as const;

export const MONEDA_DEFAULT = "CRC";
// Identidad para conversiones de misma moneda (no cambiar).
export const TIPO_CAMBIO_DEFAULT = 1;

export const TIPO_DOCUMENTO_VENTA = {
    Factura: "Factura",
    Apartado: "Apartado",
    NotaCredito: "NotaCredito",
    NotaDebito: "NotaDebito",
    Proforma: "Proforma",
} as const;

export const MODO_NOTA_CREDITO = {
    Devolucion: 0,
    CorrigeMonto: 1,
    Anulacion: 2,
} as const;

export type ModoNotaCreditoValue = (typeof MODO_NOTA_CREDITO)[keyof typeof MODO_NOTA_CREDITO];
