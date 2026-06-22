"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";
import type { CajaListadoItemDto } from "@lib/types/cajas.types";

interface CrearCajaServiceBody {
    codigo: string;
    nombre: string;
}

interface ActualizarCajaServiceBody {
    id: string;
    codigo: string;
    nombre: string;
}

export async function obtenerCajasService(): Promise<DataAPI<CajaListadoItemDto[]>> {
    return await requestAPI<CajaListadoItemDto[]>({
        url: "/cajas",
        method: "GET",
    });
}

export async function crearCajaService(
    body: CrearCajaServiceBody,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/cajas",
        method: "POST",
        body,
    });
}

export async function actualizarCajaService(
    body: ActualizarCajaServiceBody,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/cajas/${body.id}`,
        method: "PUT",
        body: {
            codigo: body.codigo,
            nombre: body.nombre,
        },
    });
}

export async function toggleEstadoCajaService(id: string): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/cajas/${id}/toggle`,
        method: "PATCH",
    });
}
