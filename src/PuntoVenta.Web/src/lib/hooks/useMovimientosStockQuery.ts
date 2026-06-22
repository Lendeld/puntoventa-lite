"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerMovimientosStockService } from "@lib/services/inventario.service";
import type { ObtenerMovimientosStockParams } from "@lib/types/inventario.types";

export function useMovimientosStockQuery(params: ObtenerMovimientosStockParams) {
    return useQuery({
        queryKey: QUERY_KEYS.inventario.movimientos(params),
        queryFn: async () => {
            const res = await obtenerMovimientosStockService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
