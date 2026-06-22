"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerNegocioService } from "@lib/services/configuracion.service";

export function useNegocioQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.configuracion.negocio,
        queryFn: async () => {
            const res = await obtenerNegocioService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 1000 * 60 * 5,
    });
}
