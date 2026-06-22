"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";
import type { EstadoTerminos } from "@lib/types/terminos.types";

export async function obtenerEstadoTerminosService(): Promise<DataAPI<EstadoTerminos>> {
    return await requestAPI<EstadoTerminos>({
        url: "/terminos/estado",
        method: "GET",
    });
}

export async function aceptarTerminosService(
    version: string,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: "/terminos/aceptar",
        method: "POST",
        body: { version },
    });
}
