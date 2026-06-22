"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerClientesService } from "@lib/services/clientes.service";
import type { ObtenerClientesPaginadoParams } from "@lib/types/clientes.types";

export function useClienteQuery(params: ObtenerClientesPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.clientes.lista(params),
        queryFn: async () => {
            const res = await obtenerClientesService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
