"use server";

import { VENDEDOR_FIELDS } from "@lib/constants/vendedores.constants";
import { actualizarVendedorSchema, crearVendedorSchema } from "@lib/schemas/vendedores.schema";
import {
    actualizarVendedorService,
    crearVendedorService,
} from "@lib/services/vendedores.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarVendedorFormValues,
    CrearVendedorFormValues,
} from "@lib/types/vendedores.types";
import { normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearVendedorAction(values: CrearVendedorFormValues): Promise<ActionResult> {
    const parsed = crearVendedorSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await crearVendedorService({
        nombre: normalizeText(values[VENDEDOR_FIELDS.NOMBRE]),
        isPrincipal: values[VENDEDOR_FIELDS.IS_PRINCIPAL],
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function actualizarVendedorAction(
    id: string,
    values: ActualizarVendedorFormValues,
): Promise<ActionResult> {
    const parsed = actualizarVendedorSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await actualizarVendedorService({
        id,
        nombre: normalizeText(values[VENDEDOR_FIELDS.NOMBRE]),
        isPrincipal: values[VENDEDOR_FIELDS.IS_PRINCIPAL],
        activo: values[VENDEDOR_FIELDS.ACTIVO],
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}

