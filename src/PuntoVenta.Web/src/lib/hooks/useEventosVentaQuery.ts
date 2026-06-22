"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerEventosVentaService } from "@lib/services/ventas.service";

export function useEventosVentaQuery(
    ventaId: string,
    options: { skip?: number; take?: number; enabled?: boolean } = {},
) {
    const skip = options.skip ?? 0;
    const take = options.take ?? 50;
    return useQuery({
        queryKey: QUERY_KEYS.ventas.eventos(ventaId, skip, take),
        queryFn: async () => {
            const res = await obtenerEventosVentaService(ventaId, { skip, take });
            if (res.errors) throw res.errors;
            return res.data ?? { items: [], total: 0, skip, take };
        },
        enabled: options.enabled !== false && Boolean(ventaId),
        staleTime: 1000 * 30,
    });
}
