"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    ObtenerVendedoresPaginadoParams,
    VendedorActivoDto,
    VendedorDto,
} from "@lib/types/vendedores.types";

interface CrearVendedorServiceParams {
    nombre: string;
    isPrincipal: boolean;
}

interface ActualizarVendedorServiceParams extends CrearVendedorServiceParams {
    id: string;
    activo: boolean;
}

export async function obtenerVendedoresService(
    params: ObtenerVendedoresPaginadoParams,
): Promise<DataAPI<PagedResult<VendedorDto>>> {
    return await requestAPI<PagedResult<VendedorDto>>({
        url: "/vendedores",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerVendedoresActivosService(): Promise<DataAPI<VendedorActivoDto[]>> {
    return await requestAPI<VendedorActivoDto[]>({
        url: "/vendedores/activos",
        method: "GET",
    });
}

export async function obtenerVendedorPorIdService(id: string): Promise<DataAPI<VendedorDto>> {
    return await requestAPI<VendedorDto>({
        url: `/vendedores/${id}`,
        method: "GET",
    });
}

export async function crearVendedorService(body: CrearVendedorServiceParams): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/vendedores",
        method: "POST",
        body,
    });
}

export async function actualizarVendedorService(
    body: ActualizarVendedorServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/vendedores/${body.id}`,
        method: "PUT",
        body: {
            nombre: body.nombre,
            isPrincipal: body.isPrincipal,
            activo: body.activo,
        },
    });
}

