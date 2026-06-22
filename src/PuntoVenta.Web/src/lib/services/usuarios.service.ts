"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    ObtenerUsuariosPaginadoParams,
    UsuarioDto,
} from "@lib/types/usuarios.types";

interface CrearUsuarioServiceParams {
    nombreUsuario: string;
    nombre: string;
    identificacion: string;
    password: string;
    rolId?: string;
    correo?: string;
    telefono?: string;
}

interface ActualizarUsuarioServiceParams {
    id: string;
    activo: boolean;
    rolId: string;
}

export async function crearUsuarioService(
    body: CrearUsuarioServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/usuarios",
        method: "POST",
        body: {
            nombreUsuario: body.nombreUsuario,
            nombre: body.nombre,
            identificacion: body.identificacion,
            password: body.password,
            rolId: body.rolId || undefined,
            correo: body.correo || undefined,
            telefono: body.telefono || undefined,
        },
    });
}

export async function obtenerUsuariosService(
    params: ObtenerUsuariosPaginadoParams,
): Promise<DataAPI<PagedResult<UsuarioDto>>> {
    return await requestAPI<PagedResult<UsuarioDto>>({
        url: "/usuarios",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerUsuarioPorIdService(
    id: string,
): Promise<DataAPI<UsuarioDto>> {
    return await requestAPI<UsuarioDto>({
        url: `/usuarios/${id}`,
        method: "GET",
    });
}

export async function actualizarUsuarioService(
    body: ActualizarUsuarioServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/usuarios/${body.id}`,
        method: "PUT",
        body: {
            activo: body.activo,
            rolId: body.rolId,
        },
    });
}

export async function toggleEstadoUsuarioService(
    id: string,
): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({
        url: `/usuarios/${id}/estado`,
        method: "PATCH",
    });
}
