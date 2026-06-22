"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerCategoriasActivasService } from "@lib/services/categorias.service";

export function useCategoriasActivasQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.categorias.activas,
        queryFn: async () => {
            const res = await obtenerCategoriasActivasService();
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
