"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerVendedoresService } from "@lib/services/vendedores.service";
import type { ObtenerVendedoresPaginadoParams } from "@lib/types/vendedores.types";

export function useVendedorQuery(params: ObtenerVendedoresPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.vendedores.lista(params),
        queryFn: async () => {
            const res = await obtenerVendedoresService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}

export function useVendedoresActivosQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.vendedores.activas,
        queryFn: async () => {
            const res = await (await import("@lib/services/vendedores.service")).obtenerVendedoresActivosService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });
}
