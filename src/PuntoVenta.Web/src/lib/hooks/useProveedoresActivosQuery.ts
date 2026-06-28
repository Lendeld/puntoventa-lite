"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerProveedoresActivosService } from "@lib/services/proveedores.service";

export function useProveedoresActivosQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.proveedores.activos,
        queryFn: async () => {
            const res = await obtenerProveedoresActivosService();
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
