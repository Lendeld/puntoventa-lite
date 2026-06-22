"use server";

import { CLIENTE_FIELDS } from "@lib/constants/clientes.constants";
import {
    actualizarClienteSchema,
    crearClienteSchema,
} from "@lib/schemas/clientes.schema";
import {
    actualizarClienteService,
    crearClienteService,
} from "@lib/services/clientes.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarClienteFormValues,
    CrearClienteFormValues,
} from "@lib/types/clientes.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

function buildClientePayload(values: CrearClienteFormValues | ActualizarClienteFormValues) {
    return {
        nombre: normalizeText(values[CLIENTE_FIELDS.NOMBRE]),
        identificacion: normalizeNullableText(values[CLIENTE_FIELDS.IDENTIFICACION]),
        correo: normalizeNullableText(values[CLIENTE_FIELDS.CORREO]),
        telefono: normalizeNullableText(values[CLIENTE_FIELDS.TELEFONO]),
        observaciones: normalizeNullableText(values[CLIENTE_FIELDS.OBSERVACIONES]),
    };
}

export async function crearClienteAction(values: CrearClienteFormValues): Promise<ActionResult> {
    const parsed = crearClienteSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await crearClienteService(buildClientePayload(values));

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}

export async function actualizarClienteAction(
    id: string,
    values: ActualizarClienteFormValues,
): Promise<ActionResult> {
    const parsed = actualizarClienteSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const response = await actualizarClienteService({
        id,
        ...buildClientePayload(values),
        activo: values[CLIENTE_FIELDS.ACTIVO],
    });

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 204, errors: undefined };
}
