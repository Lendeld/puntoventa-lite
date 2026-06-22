"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerCajasService } from "@lib/services/cajas.service";

export function useCajasQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.cajas.lista,
        queryFn: async () => {
            const res = await obtenerCajasService();
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        placeholderData: (prev) => prev,
    });
}
