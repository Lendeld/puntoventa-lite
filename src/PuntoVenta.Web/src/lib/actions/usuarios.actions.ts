"use server";

import {
    actualizarUsuarioSchema,
    crearUsuarioSchema,
} from "@lib/schemas/usuarios.schema";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import {
    actualizarUsuarioService,
    crearUsuarioService,
    toggleEstadoUsuarioService,
} from "@lib/services/usuarios.service";
import type { ActionResult } from "@lib/types/base.types";
import type {
    ActualizarUsuarioFormValues,
    CrearUsuarioFormValues,
} from "@lib/types/usuarios.types";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function crearUsuarioAction(
    values: CrearUsuarioFormValues,
): Promise<ActionResult> {
    const parsed = crearUsuarioSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await crearUsuarioService({
        nombreUsuario: parsed.data[USUARIO_FIELDS.NOMBRE_USUARIO],
        nombre: parsed.data[USUARIO_FIELDS.NOMBRE],
        identificacion: parsed.data[USUARIO_FIELDS.IDENTIFICACION],
        password: parsed.data[USUARIO_FIELDS.PASSWORD],
        rolId: parsed.data[USUARIO_FIELDS.ROL_ID] || undefined,
        correo: parsed.data[USUARIO_FIELDS.CORREO] || undefined,
        telefono: parsed.data[USUARIO_FIELDS.TELEFONO] || undefined,
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

export async function actualizarUsuarioAction(
    id: string,
    values: ActualizarUsuarioFormValues,
): Promise<ActionResult> {
    const parsed = actualizarUsuarioSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await actualizarUsuarioService({
        id,
        activo: values[USUARIO_FIELDS.ACTIVO],
        rolId: values[USUARIO_FIELDS.ROL_ID],
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

export async function toggleEstadoUsuarioAction(
    id: string,
): Promise<ActionResult> {
    const response = await toggleEstadoUsuarioService(id);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 200,
        errors: undefined,
    };
}
