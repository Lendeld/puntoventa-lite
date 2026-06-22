"use server";

import { CAJA_FIELDS } from "@lib/constants/cajas.constants";
import {
    actualizarCajaSchema,
    crearCajaSchema,
} from "@lib/schemas/cajas.schema";
import {
    actualizarCajaService,
    crearCajaService,
    toggleEstadoCajaService,
} from "@lib/services/cajas.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarCajaFormValues,
    CrearCajaFormValues,
} from "@lib/types/cajas.types";
import { normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearCajaAction(
    values: CrearCajaFormValues,
): Promise<ActionResult> {
    const parsed = crearCajaSchema.safeParse(values);
    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await crearCajaService({
        codigo: normalizeText(parsed.data[CAJA_FIELDS.CODIGO]),
        nombre: normalizeText(parsed.data[CAJA_FIELDS.NOMBRE]),
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function actualizarCajaAction(
    id: string,
    values: ActualizarCajaFormValues,
): Promise<ActionResult> {
    const parsed = actualizarCajaSchema.safeParse(values);
    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await actualizarCajaService({
        id,
        codigo: normalizeText(parsed.data[CAJA_FIELDS.CODIGO]),
        nombre: normalizeText(parsed.data[CAJA_FIELDS.NOMBRE]),
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}

export async function toggleEstadoCajaAction(id: string): Promise<ActionResult> {
    const response = await toggleEstadoCajaService(id);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 200, errors: undefined };
}
