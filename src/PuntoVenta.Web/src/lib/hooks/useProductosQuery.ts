"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerProductosService } from "@lib/services/productos.service";
import type { ObtenerProductosPaginadoParams } from "@lib/types/productos.types";

export function useProductosQuery(params: ObtenerProductosPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.productos.lista(params),
        queryFn: async () => {
            const res = await obtenerProductosService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
