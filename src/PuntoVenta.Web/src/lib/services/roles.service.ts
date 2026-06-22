"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    ObtenerRolesPaginadoParams,
    PaginaPermisosRolTabDto,
    PermisosRolPorPaginaDto,
    RolDto,
} from "@lib/types/roles.types";

interface CrearRolServiceParams {
    nombre: string;
    descripcion: string | null;
}

interface ActualizarRolServiceParams {
    id: string;
    nombre: string;
    descripcion: string | null;
    activo: boolean;
}

interface ActualizarPermisosRolServiceParams {
    id: string;
    paginaId: string;
    permisosIds: string[];
}

export async function obtenerRolesActivosService(): Promise<DataAPI<RolDto[]>> {
    return await requestAPI<RolDto[]>({
        url: "/roles/activos",
        method: "GET",
    });
}

export async function obtenerRolesService(
    params: ObtenerRolesPaginadoParams,
): Promise<DataAPI<PagedResult<RolDto>>> {
    return await requestAPI<PagedResult<RolDto>>({
        url: "/roles",
        method: "GET",
        query: {
            Pagina: params.numeroPagina,
            Tamano: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerRolPorIdService(
    id: string,
): Promise<DataAPI<RolDto>> {
    return await requestAPI<RolDto>({
        url: `/roles/${id}`,
        method: "GET",
    });
}

export async function obtenerPaginasPermisosRolService(
    id: string,
): Promise<DataAPI<PaginaPermisosRolTabDto[]>> {
    return await requestAPI<PaginaPermisosRolTabDto[]>({
        url: `/roles/${id}/permisos/paginas`,
        method: "GET",
    });
}

export async function obtenerPermisosRolPorPaginaService(
    id: string,
    paginaId: string,
): Promise<DataAPI<PermisosRolPorPaginaDto>> {
    return await requestAPI<PermisosRolPorPaginaDto>({
        url: `/roles/${id}/permisos`,
        method: "GET",
        query: {
            PaginaId: paginaId,
        },
    });
}

export async function crearRolService(
    body: CrearRolServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/roles",
        method: "POST",
        body: {
            nombre: body.nombre,
            descripcion: body.descripcion,
        },
    });
}

export async function actualizarRolService(
    body: ActualizarRolServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/roles/${body.id}`,
        method: "PUT",
        body: {
            nombre: body.nombre,
            descripcion: body.descripcion,
            activo: body.activo,
        },
    });
}

export async function actualizarPermisosRolService(
    body: ActualizarPermisosRolServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/roles/${body.id}/permisos`,
        method: "PUT",
        body: {
            paginaId: body.paginaId,
            permisosIds: body.permisosIds,
        },
    });
}

