"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";
import type { ResumenDashboardDto } from "@lib/types/dashboard.types";

export async function obtenerResumenDashboardService(): Promise<
    DataAPI<ResumenDashboardDto>
> {
    return await requestAPI<ResumenDashboardDto>({
        url: "/dashboard/resumen",
        method: "GET",
    });
}
