"use server";

import {
    actualizarUsuarioActualService,
    cambiarPasswordUsuarioActualService,
    establecerPinUsuarioActualService,
    loginService,
    validarTokenService,
} from "@/lib/services/auth.service";
import {
    guardarTokensSesionContexto,
    destruirSesion,
    obtenerSesion,
} from "@/lib/auth/sesion";
import type {
    ActualizarPerfilUsuarioActualFormValues,
    AuthFlowResponse,
    CambiarPasswordFormValues,
    EstablecerPinFormValues,
    LoginFormValues,
} from "@/lib/types/auth.types";
import {
    CAMBIAR_PASSWORD_FIELDS,
    ESTABLECER_PIN_FIELDS,
    LOGIN_FIELDS,
} from "@/lib/constants/auth.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import {
    actualizarPerfilUsuarioActualSchema,
    cambiarPasswordSchema,
    establecerPinSchema,
    loginSchema,
} from "@/lib/schemas/auth.schema";
import { ActionResult } from "@/lib/types/base.types";
import { ROUTES } from "@/lib/constants/routes.constants";
import { expiro } from "@/lib/utils/date";
import { normalizeNullableText, normalizeText } from "@/lib/utils/text.utils";
import { zodIssuesToErrors } from "@/lib/utils/zodErrors";

export interface LoginActionData {
    redirectTo?: string;
}

export interface LoginActionResult extends ActionResult {
    data: LoginActionData | null;
}

function authFlowToSessionPayload(data: {
    accessToken?: string;
    accessTokenExpiracionUtc?: string | null;
    refreshToken?: string;
    refreshTokenExpiracionUtc?: string | null;
}) {
    return {
        accessToken: data.accessToken ?? "",
        accessTokenExpiracionUtc: data.accessTokenExpiracionUtc ?? "",
        refreshToken: data.refreshToken ?? "",
        refreshTokenExpiracionUtc: data.refreshTokenExpiracionUtc ?? "",
    };
}

function hasAuthTokens(data: {
    accessToken?: string | null;
    accessTokenExpiracionUtc?: string | null;
    refreshToken?: string | null;
    refreshTokenExpiracionUtc?: string | null;
} | null | undefined): data is {
    accessToken: string;
    accessTokenExpiracionUtc: string;
    refreshToken: string;
    refreshTokenExpiracionUtc: string;
} {
    return !!(
        data?.accessToken &&
        data.accessTokenExpiracionUtc &&
        data.refreshToken &&
        data.refreshTokenExpiracionUtc
    );
}

async function persistirAuthFlow(
    data: AuthFlowResponse | null | undefined,
): Promise<LoginActionResult> {
    if (!data) {
        return {
            status: 500,
            errors: {
                general: "La respuesta del servidor no incluyó el estado de autenticación.",
            },
            data: null,
        };
    }

    if (!hasAuthTokens(data)) {
        return {
            status: 500,
            errors: {
                general: "La respuesta del servidor no incluyó los tokens de sesión.",
            },
            data: null,
        };
    }

    await guardarTokensSesionContexto(authFlowToSessionPayload(data), {
        requiereCambioPassword: data.requiresPasswordChange,
    });

    return {
        status: 204,
        errors: undefined,
        data: {
            redirectTo: data.requiresPasswordChange
                ? ROUTES.CAMBIAR_PASSWORD
                : ROUTES.HOME,
        },
    };
}

export async function loginAction(
    values: LoginFormValues,
): Promise<LoginActionResult> {
    const parsed = loginSchema.safeParse(values);
    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await loginService(
        values[LOGIN_FIELDS.NOMBRE_USUARIO],
        values[LOGIN_FIELDS.PASSWORD],
    );
    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            data: null,
        };
    }

    return persistirAuthFlow(response.data);
}

export async function logoutAction(): Promise<void> {
    await destruirSesion();
}

export async function validarTokenAction(): Promise<{ valido: boolean }> {
    const response = await validarTokenService();
    return { valido: !response.errors };
}

export async function verificarAutenticadoAction(): Promise<boolean> {
    return (await obtenerRutaPostAutenticacionAction()) !== null;
}

export async function obtenerRutaPostAutenticacionAction(): Promise<string | null> {
    const sesion = await obtenerSesion();

    if (sesion.accessToken && !expiro(sesion.accessTokenExpiracionUtc)) {
        return sesion.requiereCambioPassword
            ? ROUTES.CAMBIAR_PASSWORD
            : ROUTES.HOME;
    }

    if (sesion.refreshToken && !expiro(sesion.refreshTokenExpiracionUtc)) {
        return sesion.requiereCambioPassword
            ? ROUTES.CAMBIAR_PASSWORD
            : ROUTES.HOME;
    }

    return null;
}

export async function cambiarPasswordUsuarioActualAction(
    values: CambiarPasswordFormValues,
): Promise<LoginActionResult> {
    const parsed = cambiarPasswordSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
            data: null,
        };
    }

    const response = await cambiarPasswordUsuarioActualService(
        values[CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL],
        values[CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA],
    );

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
            data: null,
        };
    }

    return persistirAuthFlow(response.data);
}

export async function actualizarUsuarioActualAction(
    values: ActualizarPerfilUsuarioActualFormValues,
): Promise<ActionResult> {
    const parsed = actualizarPerfilUsuarioActualSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await actualizarUsuarioActualService({
        nombre: normalizeText(values[USUARIO_FIELDS.NOMBRE]),
        identificacion: normalizeText(values[USUARIO_FIELDS.IDENTIFICACION]),
        correo: normalizeNullableText(values[USUARIO_FIELDS.CORREO]),
        telefono: normalizeNullableText(values[USUARIO_FIELDS.TELEFONO]),
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

export async function establecerPinUsuarioActualAction(
    values: EstablecerPinFormValues,
): Promise<ActionResult> {
    const parsed = establecerPinSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await establecerPinUsuarioActualService(
        values[ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL],
        values[ESTABLECER_PIN_FIELDS.PIN_NUEVO],
    );

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
