"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerCatalogosVentasService,
    obtenerDocumentosVentaService,
} from "@lib/services/ventas.service";
import type { ObtenerDocumentosVentaPaginadoParams } from "@lib/types/ventas.types";

export function useDocumentosVentaQuery(
    params: ObtenerDocumentosVentaPaginadoParams,
) {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.lista(params),
        queryFn: async () => {
            const res = await obtenerDocumentosVentaService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}

export function useCatalogosVentasQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.ventas.catalogos,
        queryFn: async () => {
            const res = await obtenerCatalogosVentasService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });
}
