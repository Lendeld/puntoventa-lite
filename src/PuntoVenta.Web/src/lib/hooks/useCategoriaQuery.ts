"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerCategoriasService } from "@lib/services/categorias.service";
import type { ObtenerCategoriasPaginadoParams } from "@lib/types/categorias.types";

export function useCategoriaQuery(params: ObtenerCategoriasPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.categorias.lista(params),
        queryFn: async () => {
            const res = await obtenerCategoriasService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (previousData) => previousData,
    });
}
