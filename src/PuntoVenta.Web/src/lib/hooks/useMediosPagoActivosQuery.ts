"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerMediosPagoService } from "@lib/services/configuracion.service";

export function useMediosPagoActivosQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.configuracion.mediosPago(true),
        queryFn: async () => {
            const res = await obtenerMediosPagoService(true);
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
