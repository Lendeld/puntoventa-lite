"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerTarifasIvaService } from "@lib/services/configuracion.service";

export function useTarifasIvaActivasQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.configuracion.tarifasIva(true),
        queryFn: async () => {
            const res = await obtenerTarifasIvaService(true);
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
