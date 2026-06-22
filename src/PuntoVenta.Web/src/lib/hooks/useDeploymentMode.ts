"use client";

import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import type { DeploymentMode, UsuarioActualDto } from "@lib/types/auth.types";
import { useQuery } from "@tanstack/react-query";

interface DeploymentModeResult {
    mode: DeploymentMode | undefined;
    esLocalHost: boolean;
    esCloud: boolean;
    cargando: boolean;
}

/**
 * Modo de despliegue del API contra el que está hablando el web.
 * El backend autoriza con DeploymentPolicyBehavior; este hook es solo
 * para ocultar/gatear UI por UX. Comparte caché con el query del usuario
 * actual via tanstack/react-query, sin disparar request extra.
 */
export function useDeploymentMode(): DeploymentModeResult {
    const { data, isLoading } = useQuery({
        queryKey: QUERY_KEYS.auth.usuarioActual,
        queryFn: async () => {
            const response = await obtenerUsuarioActualService();
            if (response.errors) throw response.errors;
            return response.data as UsuarioActualDto;
        },
        staleTime: 5 * 60 * 1000,
    });

    const mode = data?.deploymentMode;
    return {
        mode,
        esLocalHost: mode === "LocalHost",
        esCloud: mode === "Cloud",
        cargando: isLoading,
    };
}
