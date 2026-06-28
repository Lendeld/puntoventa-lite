"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    ProveedorDto,
    ObtenerProveedoresPaginadoParams,
} from "@lib/types/proveedores.types";

interface CrearProveedorServiceParams {
    nombre: string;
    correo: string | null;
    telefono: string | null;
    observacion: string | null;
}

interface ActualizarProveedorServiceParams extends CrearProveedorServiceParams {
    id: string;
    activo: boolean;
}

export async function obtenerProveedoresActivosService(): Promise<DataAPI<ProveedorDto[]>> {
    return await requestAPI<ProveedorDto[]>({
        url: "/proveedores/activos",
        method: "GET",
    });
}

export async function obtenerProveedoresService(
    params: ObtenerProveedoresPaginadoParams,
): Promise<DataAPI<PagedResult<ProveedorDto>>> {
    return await requestAPI<PagedResult<ProveedorDto>>({
        url: "/proveedores",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerProveedorPorIdService(id: string): Promise<DataAPI<ProveedorDto>> {
    return await requestAPI<ProveedorDto>({
        url: `/proveedores/${id}`,
        method: "GET",
    });
}

export async function crearProveedorService(
    body: CrearProveedorServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/proveedores",
        method: "POST",
        body: {
            nombre: body.nombre,
            correo: body.correo,
            telefono: body.telefono,
            observacion: body.observacion,
        },
    });
}

export async function actualizarProveedorService(
    body: ActualizarProveedorServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/proveedores/${body.id}`,
        method: "PUT",
        body: {
            nombre: body.nombre,
            correo: body.correo,
            telefono: body.telefono,
            observacion: body.observacion,
            activo: body.activo,
        },
    });
}
