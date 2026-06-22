"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";
import type { AcercaDto } from "@lib/types/acerca.types";

export async function obtenerAcercaService(): Promise<DataAPI<AcercaDto>> {
    return await requestAPI<AcercaDto>({
        url: "/acerca",
        method: "GET",
    });
}
