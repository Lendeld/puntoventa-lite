"use server";

import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";
import {
    actualizarCategoriaSchema,
    crearCategoriaSchema,
} from "@lib/schemas/categorias.schema";
import {
    actualizarCategoriaService,
    crearCategoriaService,
} from "@lib/services/categorias.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarCategoriaFormValues,
    CrearCategoriaFormValues,
} from "@lib/types/categorias.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearCategoriaAction(
    values: CrearCategoriaFormValues,
): Promise<ActionResult> {
    const parsed = crearCategoriaSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await crearCategoriaService({
        nombre: normalizeText(values[CATEGORIA_FIELDS.NOMBRE]),
        descripcion: normalizeNullableText(values[CATEGORIA_FIELDS.DESCRIPCION]),
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function actualizarCategoriaAction(
    id: string,
    values: ActualizarCategoriaFormValues,
): Promise<ActionResult> {
    const parsed = actualizarCategoriaSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await actualizarCategoriaService({
        id,
        nombre: normalizeText(values[CATEGORIA_FIELDS.NOMBRE]),
        descripcion: normalizeNullableText(values[CATEGORIA_FIELDS.DESCRIPCION]),
        activo: values[CATEGORIA_FIELDS.ACTIVO],
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}

