"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerReporteMovimientosDineroService } from "@lib/services/ventas.service";
import type { ObtenerReporteMovimientosDineroParams } from "@lib/types/ventas.types";

export function useReporteMovimientosDineroQuery(
    params: ObtenerReporteMovimientosDineroParams,
    enabled = true,
) {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.reporteMovimientosDinero(params),
        queryFn: async () => {
            const res = await obtenerReporteMovimientosDineroService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        enabled,
        placeholderData: (previousData) => previousData,
    });
}
