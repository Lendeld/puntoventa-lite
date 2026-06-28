"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerProveedoresService } from "@lib/services/proveedores.service";
import type { ObtenerProveedoresPaginadoParams } from "@lib/types/proveedores.types";

export function useProveedorQuery(params: ObtenerProveedoresPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.proveedores.lista(params),
        queryFn: async () => {
            const res = await obtenerProveedoresService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
