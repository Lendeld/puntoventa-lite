"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerUsuariosService } from "@lib/services/usuarios.service";
import type { ObtenerUsuariosPaginadoParams } from "@lib/types/usuarios.types";

export function useUsuariosQuery(params: ObtenerUsuariosPaginadoParams) {
    return useQuery({
        queryKey: QUERY_KEYS.usuarios.lista(params),
        queryFn: async () => {
            const res = await obtenerUsuariosService(params);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        placeholderData: (prev) => prev,
    });
}
