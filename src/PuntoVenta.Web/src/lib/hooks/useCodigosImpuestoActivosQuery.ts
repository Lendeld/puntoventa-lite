"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerCodigosImpuestoService } from "@lib/services/configuracion.service";

export function useCodigosImpuestoActivosQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.configuracion.codigosImpuesto(true),
        queryFn: async () => {
            const res = await obtenerCodigosImpuestoService(true);
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
