"use server";

import {
    actualizarRolSchema,
    crearRolSchema,
} from "@lib/schemas/roles.schema";
import { ROL_FIELDS } from "@lib/constants/roles.constants";
import {
    actualizarPermisosRolService,
    actualizarRolService,
    crearRolService,
} from "@lib/services/roles.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarRolFormValues,
    CrearRolFormValues,
} from "@lib/types/roles.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearRolAction(
    values: CrearRolFormValues,
): Promise<ActionResult> {
    const parsed = crearRolSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await crearRolService({
        nombre: normalizeText(values[ROL_FIELDS.NOMBRE]),
        descripcion: normalizeNullableText(values[ROL_FIELDS.DESCRIPCION]),
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 201,
        errors: undefined,
    };
}

export async function actualizarRolAction(
    id: string,
    values: ActualizarRolFormValues,
): Promise<ActionResult> {
    const parsed = actualizarRolSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await actualizarRolService({
        id,
        nombre: normalizeText(values[ROL_FIELDS.NOMBRE]),
        descripcion: normalizeNullableText(values[ROL_FIELDS.DESCRIPCION]),
        activo: values[ROL_FIELDS.ACTIVO],
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 204,
        errors: undefined,
    };
}

export async function actualizarPermisosRolAction(
    id: string,
    paginaId: string,
    permisosIds: string[],
): Promise<ActionResult> {
    const response = await actualizarPermisosRolService({
        id,
        paginaId,
        permisosIds,
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 204,
        errors: undefined,
    };
}

