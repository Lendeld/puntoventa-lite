import {
    MONEDA_DEFAULT,
    TIPO_CAMBIO_DEFAULT,
    VENTA_FIELDS,
} from "@lib/constants/ventas.constants";
import type {
    CrearBorradorFacturaFormValues,
    DocumentoVentaDto,
    DocumentoVentaPagoForm,
} from "@lib/types/ventas.types";
import { redondear } from "@lib/utils/number.utils";
import { convertirMontoMoneda } from "@lib/utils/ventas.utils";
import dayjs from "dayjs";

function todayIsoDateTime() {
    return dayjs().toISOString();
}

export function defaultApartadoVencimiento(fechaDocumento: string) {
    const fechaBase = dayjs(fechaDocumento);
    return (fechaBase.isValid() ? fechaBase : dayjs())
        .add(30, "day")
        .toISOString();
}

export function normalizarMoneda(value: unknown) {
    return String(value ?? MONEDA_DEFAULT).toUpperCase();
}

export function normalizarTipoCambio(value: unknown) {
    const parsed =
        typeof value === "number" ? value : parseFloat(String(value ?? ""));
    return parsed > 0 ? parsed : TIPO_CAMBIO_DEFAULT;
}

export function toBaseCrc(
    montoVisual: number,
    monedaCodigo: string,
    tipoCambio: number,
) {
    return convertirMontoMoneda(montoVisual, monedaCodigo, "CRC", tipoCambio);
}

export function toMonedaDocumento(
    montoBaseCrc: number,
    monedaCodigo: string,
    tipoCambio: number,
) {
    return convertirMontoMoneda(montoBaseCrc, "CRC", monedaCodigo, tipoCambio);
}

export function buildInitialValues(): CrearBorradorFacturaFormValues {
    const fechaDocumento = todayIsoDateTime();
    return {
        [VENTA_FIELDS.CLIENTE_ID]: "",
        [VENTA_FIELDS.VENDEDOR_ID]: "",
        [VENTA_FIELDS.CAJA_ID]: "",
        [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: "01",
        [VENTA_FIELDS.FECHA_DOCUMENTO]: fechaDocumento,
        [VENTA_FIELDS.FECHA_VENCIMIENTO]: "",
        [VENTA_FIELDS.MONEDA_CODIGO]: MONEDA_DEFAULT,
        [VENTA_FIELDS.TIPO_CAMBIO]: TIPO_CAMBIO_DEFAULT,
        [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: null,
        [VENTA_FIELDS.OBSERVACIONES]: "",
        [VENTA_FIELDS.LINEAS]: [],
        [VENTA_FIELDS.PAGOS]: [],
    };
}

export function buildValuesFromDocumento(
    documento: DocumentoVentaDto,
): CrearBorradorFacturaFormValues {
    return {
        [VENTA_FIELDS.CLIENTE_ID]: documento.clienteId ?? "",
        [VENTA_FIELDS.VENDEDOR_ID]: documento.vendedorId ?? "",
        [VENTA_FIELDS.CAJA_ID]: "",
        [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: documento.condicionVentaCodigo,
        [VENTA_FIELDS.FECHA_DOCUMENTO]: dayjs(documento.fechaDocumento).toISOString(),
        [VENTA_FIELDS.FECHA_VENCIMIENTO]: documento.fechaVencimiento
            ? dayjs(documento.fechaVencimiento).toISOString()
            : dayjs(documento.fechaDocumento).add(30, "day").toISOString(),
        [VENTA_FIELDS.MONEDA_CODIGO]: documento.monedaCodigo,
        [VENTA_FIELDS.TIPO_CAMBIO]: documento.tipoCambio,
        [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: documento.plazoCreditoDias,
        [VENTA_FIELDS.OBSERVACIONES]: documento.observaciones ?? "",
        [VENTA_FIELDS.LINEAS]: documento.lineas.map((linea) => ({
            Id: linea.id,
            ProductoId: linea.productoId ?? "",
            TipoItem: linea.tipoItem,
            Codigo: linea.codigo,
            Descripcion: linea.descripcion,
            Cantidad: linea.cantidad,
            PrecioUnitario: linea.precioUnitario,
            MontoDescuento: linea.montoDescuento,
            PrecioUnitarioBaseCrc: documento.monedaCodigo === "USD"
                ? convertirMontoMoneda(linea.precioUnitario, "USD", "CRC", documento.tipoCambio)
                : linea.precioUnitario,
            MontoDescuentoBaseCrc: documento.monedaCodigo === "USD"
                ? convertirMontoMoneda(linea.montoDescuento, "USD", "CRC", documento.tipoCambio)
                : linea.montoDescuento,
            TarifaIvaImpuestoCodigo: null,
            PorcentajeImpuesto: linea.subtotal > 0
                ? redondear((linea.montoImpuesto / linea.subtotal) * 100)
                : 0,
            PermiteModificarPrecioUnitario: linea.permiteModificarPrecioUnitario,
            NoAplicaExistencias: linea.noAplicaExistencias,
        })),
        [VENTA_FIELDS.PAGOS]: [],
    };
}

export function buildEmptyPago(monedaDocumento: string): DocumentoVentaPagoForm {
    return {
        MonedaCodigo: monedaDocumento,
        TipoCambioAplicado: 1,
        MedioPagoCodigo: "",
        MontoEntregado: 0,
        MontoAplicadoMonedaPago: 0,
        MontoAplicadoDocumento: 0,
        MontoVueltoMonedaPago: 0,
        MontoVueltoDocumento: 0,
        Referencia: "",
        Observacion: "",
    };
}

export function resolveDefaultMedioPagoCodigo(
    mediosPago: Array<{ codigo: string; detalle: string }>,
) {
    const medioEfectivo = mediosPago.find((medio) => {
        const codigo = medio.codigo.trim().toUpperCase();
        const detalle = medio.detalle.trim().toUpperCase();

        return codigo.includes("EFE") || detalle.includes("EFECTIVO");
    });

    return medioEfectivo?.codigo ?? mediosPago[0]?.codigo ?? "";
}
