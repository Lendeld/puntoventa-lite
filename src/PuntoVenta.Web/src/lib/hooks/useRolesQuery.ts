"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerRolesService } from "@lib/services/roles.service";
import type { ObtenerRolesPaginadoParams } from "@lib/types/roles.types";

export function useRolesQuery(params: ObtenerRolesPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.roles.lista(params),
        queryFn: async () => {
            const res = await obtenerRolesService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
