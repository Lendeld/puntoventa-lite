"use server";

import type {
    ActualizarPerfilUsuarioActualDto,
    AuthFlowResponse,
    UsuarioActualDto,
} from "@lib/types/auth.types";
import { requestAPI } from "@lib/utils/requestApi";
import { DataAPI } from "@lib/types/base.types";


export async function loginService(
    nombreUsuario: string,
    password: string,
): Promise<DataAPI<AuthFlowResponse>> {
    return await requestAPI<AuthFlowResponse>({
        url: "/auth/login",
        method: "POST",
        body: { nombreUsuario, password },
        skipAuth: true,
    });
}

export async function refreshTokenService(
    refreshToken: string,
): Promise<DataAPI<AuthFlowResponse>> {
    return await requestAPI<AuthFlowResponse>({
        url: "/auth/refresh",
        method: "POST",
        body: { refreshToken },
        skipAuth: true,
    });
}

export async function validarTokenService(): Promise<DataAPI> {
    return await requestAPI({
        url: "/auth/validar-token",
        method: "GET",
    });
}

export async function logoutService(refreshToken?: string): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: "/auth/logout",
        method: "POST",
        body: { refreshToken },
    });
}

export async function validarPermisoService(
    permiso: string,
): Promise<DataAPI> {
    return await requestAPI({
        url: `/auth/validar-permiso/${encodeURIComponent(permiso)}`,
        method: "GET",
    });
}

export async function obtenerUsuarioActualService(): Promise<DataAPI<UsuarioActualDto>> {
    return await requestAPI<UsuarioActualDto>({
        url: "/auth/usuario-actual",
        method: "GET",
    });
}

export async function actualizarUsuarioActualService(
    body: ActualizarPerfilUsuarioActualDto,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: "/auth/usuario-actual",
        method: "PUT",
        body: {
            nombre: body.nombre,
            identificacion: body.identificacion,
            correo: body.correo,
            telefono: body.telefono,
        },
    });
}

export async function cambiarPasswordUsuarioActualService(
    passwordActual: string,
    passwordNueva: string,
): Promise<DataAPI<AuthFlowResponse>> {
    return await requestAPI<AuthFlowResponse>({
        url: "/auth/cambiar-password",
        method: "PUT",
        body: {
            passwordActual,
            passwordNueva,
        },
    });
}

export async function establecerPinUsuarioActualService(
    passwordActual: string,
    pinNuevo: string,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: "/auth/pin",
        method: "PUT",
        body: {
            passwordActual,
            pinNuevo,
        },
    });
}
