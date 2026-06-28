"use server";

import { PROVEEDOR_FIELDS } from "@lib/constants/proveedores.constants";
import {
    actualizarProveedorSchema,
    crearProveedorSchema,
} from "@lib/schemas/proveedores.schema";
import {
    actualizarProveedorService,
    crearProveedorService,
} from "@lib/services/proveedores.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarProveedorFormValues,
    CrearProveedorFormValues,
} from "@lib/types/proveedores.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearProveedorAction(
    values: CrearProveedorFormValues,
): Promise<ActionResult> {
    const parsed = crearProveedorSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await crearProveedorService({
        nombre: normalizeText(values[PROVEEDOR_FIELDS.NOMBRE]),
        correo: normalizeNullableText(values[PROVEEDOR_FIELDS.CORREO]),
        telefono: normalizeNullableText(values[PROVEEDOR_FIELDS.TELEFONO]),
        observacion: normalizeNullableText(values[PROVEEDOR_FIELDS.OBSERVACION]),
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function actualizarProveedorAction(
    id: string,
    values: ActualizarProveedorFormValues,
): Promise<ActionResult> {
    const parsed = actualizarProveedorSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await actualizarProveedorService({
        id,
        nombre: normalizeText(values[PROVEEDOR_FIELDS.NOMBRE]),
        correo: normalizeNullableText(values[PROVEEDOR_FIELDS.CORREO]),
        telefono: normalizeNullableText(values[PROVEEDOR_FIELDS.TELEFONO]),
        observacion: normalizeNullableText(values[PROVEEDOR_FIELDS.OBSERVACION]),
        activo: values[PROVEEDOR_FIELDS.ACTIVO],
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}
