"use server";

import {
    abonarApartadoSchema,
    crearApartadoSchema,
    crearBorradorFacturaSchema,
    crearFacturaSchema,
    extenderVencimientoApartadoSchema,
} from "@lib/schemas/ventas.schema";
import {
    abonarApartadoService,
    actualizarProformaService,
    anularAbonoFacturaService,
    cancelarApartadoService,
    convertirApartadoAFacturaService,
    crearApartadoService,
    crearFacturaService,
    crearProformaService,
    emitirNotaCreditoService,
    emitirNotaDebitoService,
    extenderVencimientoApartadoService,
    facturarProformaService,
    obtenerDocumentoVentaPorIdService,
    registrarAbonoFacturaService,
} from "@lib/services/ventas.service";
import { MONEDA_DEFAULT, VENTA_FIELDS } from "@lib/constants/ventas.constants";
import type {
    CrearBorradorFacturaFormValues,
    DocumentoVentaLineaForm,
    EmitirFacturaResult,
    EmitirFacturaPayload,
    EmitirNotaCreditoLineaPayload,
    GuardarBorradorFacturaResult,
    ModoNotaCreditoCode,
    RegistrarAbonoFacturaResult,
    AnularAbonoFacturaResult,
    VentaActionResult,
} from "@lib/types/ventas.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";
import dayjs from "dayjs";
import { revalidatePath } from "next/cache";

function normalizeNullableGuid(value: string | null | undefined) {
    const normalized = normalizeNullableText(value);
    return normalized && normalized !== "" ? normalized : null;
}

function buildPayload(values: CrearBorradorFacturaFormValues) {
    const fechaDocumento = normalizeText(values[VENTA_FIELDS.FECHA_DOCUMENTO]);
    const lineas = values[VENTA_FIELDS.LINEAS] as DocumentoVentaLineaForm[];

    return {
        clienteId: normalizeNullableGuid(values[VENTA_FIELDS.CLIENTE_ID]),
        vendedorId: normalizeNullableGuid(values[VENTA_FIELDS.VENDEDOR_ID]),
        cajaId: normalizeNullableGuid(values[VENTA_FIELDS.CAJA_ID]),
        condicionVentaCodigo: normalizeText(values[VENTA_FIELDS.CONDICION_VENTA_CODIGO]),
        fechaDocumento: dayjs(fechaDocumento).toISOString(),
        fechaVencimiento: normalizeNullableText(values[VENTA_FIELDS.FECHA_VENCIMIENTO])
            ? dayjs(normalizeText(values[VENTA_FIELDS.FECHA_VENCIMIENTO])).toISOString()
            : null,
        monedaCodigo: normalizeText(values[VENTA_FIELDS.MONEDA_CODIGO] || MONEDA_DEFAULT).toUpperCase(),
        tipoCambio: values[VENTA_FIELDS.TIPO_CAMBIO],
        plazoCreditoDias: values[VENTA_FIELDS.PLAZO_CREDITO_DIAS] ?? null,
        observaciones: normalizeNullableText(values[VENTA_FIELDS.OBSERVACIONES]),
        lineas: lineas.map((linea) => ({
            id: linea.Id ?? null,
            productoId: linea.ProductoId,
            cantidad: linea.Cantidad,
            precioUnitario: linea.PrecioUnitario,
            montoDescuento: linea.MontoDescuento,
            devuelveInventario: false,
            descripcion: normalizeNullableText(linea.Descripcion),
        })),
        pagos: values[VENTA_FIELDS.PAGOS].map((pago) => ({
            monedaCodigo: normalizeText(pago.MonedaCodigo).toUpperCase(),
            tipoCambioAplicado: pago.TipoCambioAplicado,
            medioPagoCodigo: normalizeText(pago.MedioPagoCodigo),
            montoEntregado: pago.MontoEntregado,
            montoAplicadoMonedaPago: pago.MontoAplicadoMonedaPago,
            montoAplicadoDocumento: pago.MontoAplicadoDocumento,
            montoVueltoMonedaPago: pago.MontoVueltoMonedaPago,
            montoVueltoDocumento: pago.MontoVueltoDocumento,
            referencia: normalizeNullableText(pago.Referencia),
            observacion: normalizeNullableText(pago.Observacion),
        })),
    };
}

function buildProformaPayload(values: CrearBorradorFacturaFormValues) {
    const payload = buildPayload(values);
    const { pagos: _pagos, ...proformaPayload } = payload;
    return proformaPayload;
}

function buildEmitirPayload(values: CrearBorradorFacturaFormValues): EmitirFacturaPayload {
    return {
        cajaId: normalizeNullableGuid(values[VENTA_FIELDS.CAJA_ID]),
        pagos: values[VENTA_FIELDS.PAGOS].map((pago) => ({
            monedaCodigo: normalizeText(pago.MonedaCodigo).toUpperCase(),
            tipoCambioAplicado: pago.TipoCambioAplicado,
            medioPagoCodigo: normalizeText(pago.MedioPagoCodigo),
            montoEntregado: pago.MontoEntregado,
            montoAplicadoMonedaPago: pago.MontoAplicadoMonedaPago,
            montoAplicadoDocumento: pago.MontoAplicadoDocumento,
            montoVueltoMonedaPago: pago.MontoVueltoMonedaPago,
            montoVueltoDocumento: pago.MontoVueltoDocumento,
            referencia: normalizeNullableText(pago.Referencia),
            observacion: normalizeNullableText(pago.Observacion),
        })),
    };
}

async function obtenerDetalleResult(id: string, status: number): Promise<VentaActionResult<EmitirFacturaResult>> {
    const detalleResponse = await obtenerDocumentoVentaPorIdService(id);

    return {
        status,
        errors: undefined,
        data: {
            id,
            detalle: detalleResponse.errors ? null : (detalleResponse.data ?? null),
        },
    };
}

export async function crearFacturaAction(
    values: CrearBorradorFacturaFormValues,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const parsed = crearFacturaSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await crearFacturaService(buildPayload(values));

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 201);
}

export async function crearProformaAction(
    values: CrearBorradorFacturaFormValues,
): Promise<VentaActionResult<GuardarBorradorFacturaResult>> {
    const parsed = crearBorradorFacturaSchema.safeParse({
        ...values,
        [VENTA_FIELDS.PAGOS]: [],
    });

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await crearProformaService(buildProformaPayload(values));

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 201);
}

export async function crearApartadoAction(
    values: CrearBorradorFacturaFormValues,
): Promise<VentaActionResult<GuardarBorradorFacturaResult>> {
    const parsed = crearApartadoSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await crearApartadoService(buildPayload(values));

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 201);
}

export async function actualizarProformaAction(
    id: string,
    values: CrearBorradorFacturaFormValues,
): Promise<VentaActionResult<GuardarBorradorFacturaResult>> {
    const parsed = crearBorradorFacturaSchema.safeParse({
        ...values,
        [VENTA_FIELDS.PAGOS]: [],
    });

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await actualizarProformaService(id, buildProformaPayload(values));

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 200);
}

export async function facturarProformaAction(
    id: string,
    values: CrearBorradorFacturaFormValues,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const payload = buildEmitirPayload(values);
    const response = await facturarProformaService(id, payload);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 200);
}

export async function abonarApartadoAction(
    id: string,
    pago: CrearBorradorFacturaFormValues[typeof VENTA_FIELDS.PAGOS][number],
    fechaPago?: string,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const parsed = abonarApartadoSchema.safeParse({
        fechaPago: fechaPago ?? "",
        medioPagoCodigo: normalizeText(pago.MedioPagoCodigo),
        monto: pago.MontoAplicadoDocumento,
        referencia: pago.Referencia ?? "",
        observacion: pago.Observacion ?? "",
    });

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await abonarApartadoService(id, {
        fechaPago: dayjs(parsed.data.fechaPago).toISOString(),
        pago: {
            monedaCodigo: normalizeText(pago.MonedaCodigo).toUpperCase(),
            tipoCambioAplicado: pago.TipoCambioAplicado,
            medioPagoCodigo: parsed.data.medioPagoCodigo,
            montoEntregado: pago.MontoEntregado,
            montoAplicadoMonedaPago: pago.MontoAplicadoMonedaPago,
            montoAplicadoDocumento: pago.MontoAplicadoDocumento,
            montoVueltoMonedaPago: pago.MontoVueltoMonedaPago,
            montoVueltoDocumento: pago.MontoVueltoDocumento,
            referencia: normalizeNullableText(parsed.data.referencia),
            observacion: normalizeNullableText(parsed.data.observacion),
        },
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(id, 200);
}

export interface RegistrarAbonoFacturaPayload {
    monedaCodigo: string;
    medioPagoCodigo: string;
    monto: number;
    referencia?: string | null;
    observacion?: string | null;
    fechaPago?: string | null;
}

export async function registrarAbonoFacturaAction(
    id: string,
    payload: RegistrarAbonoFacturaPayload,
): Promise<VentaActionResult<RegistrarAbonoFacturaResult>> {
    if (!payload.monto || payload.monto <= 0) {
        return {
            status: 400,
            errors: {
                DocumentoVentaPago_MontoAplicadoDocumento: "El monto debe ser mayor a cero.",
            },
            data: null,
        };
    }

    const moneda = (payload.monedaCodigo || MONEDA_DEFAULT).toUpperCase();
    const monto = payload.monto;

    const response = await registrarAbonoFacturaService(id, {
        fechaPago: payload.fechaPago ? dayjs(payload.fechaPago).toISOString() : null,
        pago: {
            monedaCodigo: moneda,
            tipoCambioAplicado: 1,
            medioPagoCodigo: normalizeText(payload.medioPagoCodigo),
            montoEntregado: monto,
            montoAplicadoMonedaPago: monto,
            montoAplicadoDocumento: monto,
            montoVueltoMonedaPago: 0,
            montoVueltoDocumento: 0,
            referencia: normalizeNullableText(payload.referencia),
            observacion: normalizeNullableText(payload.observacion),
        },
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return {
        status: 200,
        errors: undefined,
        data: { pagoId: response.data! },
    };
}

export async function anularAbonoFacturaAction(
    documentoId: string,
    pagoId: string,
    motivo: string,
): Promise<VentaActionResult<AnularAbonoFacturaResult>> {
    const motivoNormalizado = motivo.trim();

    if (!motivoNormalizado || motivoNormalizado.length < 3) {
        return {
            status: 400,
            errors: {
                DocumentoVentaPago_MotivoAnulacion: "El motivo debe tener al menos 3 caracteres.",
            },
            data: null,
        };
    }

    if (motivoNormalizado.length > 255) {
        return {
            status: 400,
            errors: {
                DocumentoVentaPago_MotivoAnulacion: "El motivo no puede exceder 255 caracteres.",
            },
            data: null,
        };
    }

    const response = await anularAbonoFacturaService(documentoId, pagoId, {
        motivo: motivoNormalizado,
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return {
        status: 200,
        errors: undefined,
        data: { pagoId: response.data! },
    };
}

export async function extenderVencimientoApartadoAction(
    id: string,
    fechaVencimiento: string,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const parsed = extenderVencimientoApartadoSchema.safeParse({ fechaVencimiento });
    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await extenderVencimientoApartadoService(id, {
        fechaVencimiento: dayjs(parsed.data.fechaVencimiento).toISOString(),
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(id, 200);
}

export async function convertirApartadoAFacturaAction(
    id: string,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const response = await convertirApartadoAFacturaService(id);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(response.data!, 200);
}

export async function cancelarApartadoAction(
    id: string,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const response = await cancelarApartadoService(id);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    return await obtenerDetalleResult(id, 200);
}

export interface EmitirNotaCreditoActionPayload {
    documentoOrigenId: string;
    modo: ModoNotaCreditoCode;
    lineas: EmitirNotaCreditoLineaPayload[];
    razon: string;
    observaciones?: string | null;
    productosSinReintegro?: string[];
}

export async function emitirNotaCreditoAction(
    payload: EmitirNotaCreditoActionPayload,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const response = await emitirNotaCreditoService({
        documentoOrigenId: payload.documentoOrigenId,
        modo: payload.modo,
        lineas: payload.lineas.map((linea) => ({
            productoId: linea.productoId,
            cantidad: linea.cantidad,
            precioUnitario: linea.precioUnitario ?? null,
            montoDescuento: linea.montoDescuento ?? 0,
            descripcion: normalizeNullableText(linea.descripcion),
        })),
        razon: normalizeText(payload.razon),
        observaciones: normalizeNullableText(payload.observaciones),
        productosSinReintegro: payload.productosSinReintegro,
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    // Invalida cache del segmento de la factura origen y del listado para que
    // al volver al detalle aparezca la NC en "Documentos generados".
    revalidatePath(`/emision/ventas/${payload.documentoOrigenId}`);
    revalidatePath("/emision/ventas");

    return await obtenerDetalleResult(response.data!, 201);
}

export interface EmitirNotaDebitoActionPayload {
    documentoOrigenId: string;
    lineas: EmitirNotaCreditoLineaPayload[];
    razon: string;
    observaciones?: string | null;
}

export async function emitirNotaDebitoAction(
    payload: EmitirNotaDebitoActionPayload,
): Promise<VentaActionResult<EmitirFacturaResult>> {
    const response = await emitirNotaDebitoService({
        documentoOrigenId: payload.documentoOrigenId,
        lineas: payload.lineas.map((linea) => ({
            productoId: linea.productoId,
            cantidad: linea.cantidad,
            precioUnitario: linea.precioUnitario ?? null,
            montoDescuento: linea.montoDescuento ?? 0,
            descripcion: normalizeNullableText(linea.descripcion),
        })),
        razon: normalizeText(payload.razon),
        observaciones: normalizeNullableText(payload.observaciones),
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            severity: response.errors.severity,
            data: null,
        };
    }

    // Invalida cache del segmento de la factura origen y del listado para que
    // al volver al detalle aparezca la ND en "Documentos generados".
    revalidatePath(`/emision/ventas/${payload.documentoOrigenId}`);
    revalidatePath("/emision/ventas");

    return await obtenerDetalleResult(response.data!, 201);
}
