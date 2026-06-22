"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerReporteVentasRangoService } from "@lib/services/ventas.service";
import type { ObtenerReporteVentasRangoParams } from "@lib/types/ventas.types";

export function useReporteVentasRangoQuery(
    params: ObtenerReporteVentasRangoParams,
    enabled: boolean,
) {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.reporteRango(params),
        queryFn: async () => {
            const res = await obtenerReporteVentasRangoService(params);
            if (res.errors) throw res.errors;
            return res.data;
        },
        enabled,
        placeholderData: (prev) => prev,
    });
}
