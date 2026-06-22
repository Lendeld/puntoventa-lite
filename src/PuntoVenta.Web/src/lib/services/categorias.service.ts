"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    CategoriaDto,
    ObtenerCategoriasPaginadoParams,
} from "@lib/types/categorias.types";

interface CrearCategoriaServiceParams {
    nombre: string;
    descripcion: string | null;
}

interface ActualizarCategoriaServiceParams extends CrearCategoriaServiceParams {
    id: string;
    activo: boolean;
}

export async function obtenerCategoriasActivasService(): Promise<DataAPI<CategoriaDto[]>> {
    return await requestAPI<CategoriaDto[]>({
        url: "/categorias/activos",
        method: "GET",
    });
}

export async function obtenerCategoriasService(
    params: ObtenerCategoriasPaginadoParams,
): Promise<DataAPI<PagedResult<CategoriaDto>>> {
    return await requestAPI<PagedResult<CategoriaDto>>({
        url: "/categorias",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerCategoriaPorIdService(id: string): Promise<DataAPI<CategoriaDto>> {
    return await requestAPI<CategoriaDto>({
        url: `/categorias/${id}`,
        method: "GET",
    });
}

export async function crearCategoriaService(
    body: CrearCategoriaServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/categorias",
        method: "POST",
        body: {
            nombre: body.nombre,
            descripcion: body.descripcion,
        },
    });
}

export async function actualizarCategoriaService(
    body: ActualizarCategoriaServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/categorias/${body.id}`,
        method: "PUT",
        body: {
            nombre: body.nombre,
            descripcion: body.descripcion,
            activo: body.activo,
        },
    });
}

