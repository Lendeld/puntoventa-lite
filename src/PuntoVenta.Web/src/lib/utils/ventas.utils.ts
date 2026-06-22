import { CONDICION_VENTA_CONTADO, CONDICIONES_VENTA_CREDITO } from "@lib/constants/ventas.constants";
import { redondear, redondearMoneda } from "@lib/utils/number.utils";
import type { DocumentoVentaLineaForm, DocumentoVentaPagoForm, FacturaTotales } from "@lib/types/ventas.types";

export const ESTADO_DOCUMENTO = {
    BORRADOR: "Borrador",
    EMITIDO: "Emitido",
    ANULADO: "Anulado",
    RESERVADO: "Reservado",
    CONVERTIDO: "Convertido",
    CANCELADO: "Cancelado",
    VENCIDO: "Vencido",
} as const;

function esCondicionCredito(codigo: string) {
    return (CONDICIONES_VENTA_CREDITO as readonly string[]).includes(codigo);
}

export function requierePagoCompleto(codigo: string) {
    return codigo === CONDICION_VENTA_CONTADO;
}

export function calcularTotalesFactura(
    lineas: DocumentoVentaLineaForm[],
    pagos: DocumentoVentaPagoForm[],
): FacturaTotales {
    // Replicar exacto la matemática por línea del backend
    // (DocumentoVentaLinea.Crear): precio y montos a 5 decimales
    // (half-away-from-zero), igual que la columna numeric(18,5). Se acumula la
    // precisión completa y se redondea a 2 dec SOLO al final (lo que se muestra
    // y se cobra). Si se redondeara a 2 por línea, el total divergiría del
    // TotalComprobante del backend (5 dec) y el cobro no cuadraría.
    let subtotalPreciso = 0;
    let impuestoPreciso = 0;
    for (const linea of lineas) {
        const precio = redondear(linea.PrecioUnitario);
        const bruto = redondear(linea.Cantidad * precio);
        const neto = Math.max(redondear(bruto - linea.MontoDescuento), 0);
        const montoImpuesto = redondear(neto * ((linea.PorcentajeImpuesto ?? 0) / 100));
        subtotalPreciso += neto;
        impuestoPreciso += montoImpuesto;
    }

    const subtotal = redondearMoneda(subtotalPreciso);
    const descuentos = redondearMoneda(lineas.reduce((acc, linea) => acc + linea.MontoDescuento, 0));
    const impuesto = redondearMoneda(impuestoPreciso);
    // Total a pagar = total preciso (5 dec) redondeado a 2 dec en el borde.
    const total = redondearMoneda(subtotalPreciso + impuestoPreciso);
    const pagado = redondearMoneda(pagos.reduce((acc, pago) => acc + pago.MontoAplicadoDocumento, 0));
    const saldo = redondearMoneda(total - pagado);

    return {
        subtotal,
        descuentos,
        impuesto,
        total,
        pagado,
        saldo,
    };
}

export function obtenerPorcentajeImpuesto(
    _codigoImpuestoCodigo: string | null | undefined,
    tarifaIvaImpuestoCodigo: string | null | undefined,
    tarifas: Array<{ codigo: string; porcentaje: number }>,
) {
    if (!tarifaIvaImpuestoCodigo) {
        return 0;
    }

    return tarifas.find((tarifa) => tarifa.codigo === tarifaIvaImpuestoCodigo)?.porcentaje ?? 0;
}

export function puedeEmitirFactura(params: {
    estado: string | null;
    condicionVentaCodigo: string;
    plazoCreditoDias: number | null;
    totales: FacturaTotales;
    valid: boolean;
}) {
    if (params.estado === ESTADO_DOCUMENTO.EMITIDO) return false;
    if (!params.valid) return false;

    if (esCondicionCredito(params.condicionVentaCodigo)) {
        return (params.plazoCreditoDias ?? 0) > 0;
    }

    if (requierePagoCompleto(params.condicionVentaCodigo)) {
        return redondear(params.totales.saldo) === 0;
    }

    return true;
}

export function formatMonedaPorCodigo(value: number, monedaCodigo = "CRC") {
    const simbolo = monedaCodigo.toUpperCase() === "USD" ? "$" : "₡";

    return `${simbolo} ${value.toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    })}`;
}

export function convertirMontoMoneda(
    value: number,
    monedaOrigen: string,
    monedaDestino: string,
    tipoCambio: number,
) {
    const origen = monedaOrigen.toUpperCase();
    const destino = monedaDestino.toUpperCase();
    const cambio = tipoCambio > 0 ? tipoCambio : 1;

    if (origen === destino) return redondear(value);
    if (origen === "CRC" && destino === "USD") return redondear(value / cambio);
    if (origen === "USD" && destino === "CRC") return redondear(value * cambio);

    return redondear(value);
}

function resolverTipoCambioPago(
    _monedaPago: string,
    _monedaDocumento: string,
    tipoCambioDocumento: number,
) {
    return tipoCambioDocumento > 0 ? tipoCambioDocumento : 1;
}

export function recalcularPagosDocumento(
    pagos: DocumentoVentaPagoForm[],
    monedaDocumento: string,
    tipoCambioDocumento: number,
    totalDocumento: number,
): DocumentoVentaPagoForm[] {
    const monedaDoc = monedaDocumento.toUpperCase();
    let restanteDocumento = redondearMoneda(totalDocumento);

    return pagos.map((pago) => {
        const monedaPago = String(pago.MonedaCodigo || monedaDoc).toUpperCase();
        const tipoCambioAplicado = resolverTipoCambioPago(
            monedaPago,
            monedaDoc,
            tipoCambioDocumento,
        );
        const montoEntregado = redondearMoneda(Number(pago.MontoEntregado) || 0);

        if (montoEntregado <= 0 || restanteDocumento <= 0) {
            return {
                ...pago,
                MonedaCodigo: monedaPago,
                TipoCambioAplicado: tipoCambioAplicado,
                MontoEntregado: montoEntregado,
                MontoAplicadoMonedaPago: 0,
                MontoAplicadoDocumento: 0,
                MontoVueltoMonedaPago: montoEntregado > 0 ? montoEntregado : 0,
                MontoVueltoDocumento:
                    montoEntregado > 0
                        ? redondearMoneda(
                              convertirMontoMoneda(
                                  montoEntregado,
                                  monedaPago,
                                  monedaDoc,
                                  tipoCambioAplicado,
                              ),
                          )
                        : 0,
            };
        }

        const entregadoDocumento = convertirMontoMoneda(
            montoEntregado,
            monedaPago,
            monedaDoc,
            tipoCambioAplicado,
        );
        const montoAplicadoDocumento = redondearMoneda(
            Math.min(entregadoDocumento, restanteDocumento),
        );
        const montoAplicadoMonedaPago = redondearMoneda(
            convertirMontoMoneda(
                montoAplicadoDocumento,
                monedaDoc,
                monedaPago,
                tipoCambioAplicado,
            ),
        );
        const montoVueltoMonedaPago = redondearMoneda(
            Math.max(montoEntregado - montoAplicadoMonedaPago, 0),
        );
        const montoVueltoDocumento = redondearMoneda(
            convertirMontoMoneda(
                montoVueltoMonedaPago,
                monedaPago,
                monedaDoc,
                tipoCambioAplicado,
            ),
        );

        restanteDocumento = redondearMoneda(
            Math.max(restanteDocumento - montoAplicadoDocumento, 0),
        );

        return {
            ...pago,
            MonedaCodigo: monedaPago,
            TipoCambioAplicado: tipoCambioAplicado,
            MontoEntregado: montoEntregado,
            MontoAplicadoMonedaPago: montoAplicadoMonedaPago,
            MontoAplicadoDocumento: montoAplicadoDocumento,
            MontoVueltoMonedaPago: montoVueltoMonedaPago,
            MontoVueltoDocumento: montoVueltoDocumento,
        };
    });
}

export function getEstadoDocumentoLabel(estado: string | null | undefined) {
    switch (estado) {
        case ESTADO_DOCUMENTO.BORRADOR:
            return "Borrador";
        case ESTADO_DOCUMENTO.EMITIDO:
            return "Emitido";
        case ESTADO_DOCUMENTO.ANULADO:
            return "Anulado";
        case ESTADO_DOCUMENTO.RESERVADO:
            return "Reservado";
        case ESTADO_DOCUMENTO.CONVERTIDO:
            return "Convertido";
        case ESTADO_DOCUMENTO.CANCELADO:
            return "Cancelado";
        case ESTADO_DOCUMENTO.VENCIDO:
            return "Vencido";
        default:
            return "Sin guardar";
    }
}
