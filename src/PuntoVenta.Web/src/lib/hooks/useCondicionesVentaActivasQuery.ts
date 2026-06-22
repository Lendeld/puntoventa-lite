"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerCondicionesVentaService } from "@lib/services/configuracion.service";

export function useCondicionesVentaActivasQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.configuracion.condicionesVenta(true),
        queryFn: async () => {
            const res = await obtenerCondicionesVentaService(true);
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
