"use server";

import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import { crearProductoSchema, editarProductoSchema } from "@lib/schemas/productos.schema";
import {
    actualizarProductoService,
    crearProductoService,
    toggleEstadoProductoService,
} from "@lib/services/productos.service";
import type { ActionResult } from "@lib/types/base.types";
import type { CrearProductoFormValues, EditarProductoFormValues } from "@lib/types/productos.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearProductoAction(values: CrearProductoFormValues): Promise<ActionResult> {
    const parsed = crearProductoSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const body = {
        codigo: normalizeText(values[PRODUCTO_FIELDS.CODIGO]),
        nombre: normalizeText(values[PRODUCTO_FIELDS.NOMBRE]),
        tipoItem: values[PRODUCTO_FIELDS.TIPO_ITEM],
        precioUnitario: values[PRODUCTO_FIELDS.PRECIO_UNITARIO],
        codigoBarras: normalizeNullableText(values[PRODUCTO_FIELDS.CODIGO_BARRAS]),
        descripcion: normalizeNullableText(values[PRODUCTO_FIELDS.DESCRIPCION]),
        precioCosto: values[PRODUCTO_FIELDS.PRECIO_COSTO] ?? null,
        categoriaId: values[PRODUCTO_FIELDS.CATEGORIA_ID] || null,
        proveedorId: values[PRODUCTO_FIELDS.PROVEEDOR_ID] || null,
        tarifaIvaImpuestoCodigo: values[PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO] || null,
        noAplicaExistencias:
            values[PRODUCTO_FIELDS.TIPO_ITEM] === 1
                ? (values[PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS] ?? false)
                : undefined,
        permiteModificarPrecioUnitario:
            values[PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO] ?? false,
        // Stock inicial: solo para bienes que aplican existencias.
        existenciaInicial:
            values[PRODUCTO_FIELDS.TIPO_ITEM] === 1 &&
            !(values[PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS] ?? false)
                ? (values[PRODUCTO_FIELDS.EXISTENCIA_INICIAL] ?? null)
                : null,
    };

    const response = await crearProductoService(body);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function editarProductoAction(
    id: string,
    values: EditarProductoFormValues,
): Promise<ActionResult> {
    const parsed = editarProductoSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const body = {
        id,
        codigo: normalizeText(values[PRODUCTO_FIELDS.CODIGO]),
        nombre: normalizeText(values[PRODUCTO_FIELDS.NOMBRE]),
        tipoItem: values[PRODUCTO_FIELDS.TIPO_ITEM],
        precioUnitario: values[PRODUCTO_FIELDS.PRECIO_UNITARIO],
        codigoBarras: normalizeNullableText(values[PRODUCTO_FIELDS.CODIGO_BARRAS]),
        descripcion: normalizeNullableText(values[PRODUCTO_FIELDS.DESCRIPCION]),
        precioCosto: values[PRODUCTO_FIELDS.PRECIO_COSTO] ?? null,
        categoriaId: values[PRODUCTO_FIELDS.CATEGORIA_ID] || null,
        proveedorId: values[PRODUCTO_FIELDS.PROVEEDOR_ID] || null,
        tarifaIvaImpuestoCodigo: values[PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO] || null,
        noAplicaExistencias:
            values[PRODUCTO_FIELDS.TIPO_ITEM] === 1
                ? (values[PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS] ?? false)
                : undefined,
        permiteModificarPrecioUnitario:
            values[PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO] ?? false,
    };

    const response = await actualizarProductoService(body);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}

export async function toggleEstadoProductoAction(id: string): Promise<ActionResult> {
    const response = await toggleEstadoProductoService(id);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 200, errors: undefined };
}
