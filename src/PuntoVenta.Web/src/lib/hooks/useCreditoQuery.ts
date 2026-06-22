"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerFacturasCreditoService,
} from "@lib/services/ventas.service";
import { obtenerSaldoCreditoClienteService } from "@lib/services/clientes.service";
import type { ObtenerFacturasCreditoParams } from "@lib/types/ventas.types";

export function useFacturasCreditoQuery(params: ObtenerFacturasCreditoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.credito(params),
        queryFn: async () => {
            const res = await obtenerFacturasCreditoService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}

export function useSaldoCreditoClienteQuery(clienteId: string | null | undefined) {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.saldoCliente(clienteId ?? ""),
        queryFn: async () => {
            if (!clienteId) throw new Error("clienteId requerido");
            const res = await obtenerSaldoCreditoClienteService(clienteId);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        enabled: !!clienteId,
    });
}
