"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerRolesActivosService } from "@lib/services/roles.service";

export function useRolesActivosQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.roles.activos,
        queryFn: async () => {
            const res = await obtenerRolesActivosService();
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 1000 * 60 * 5,
    });
}
