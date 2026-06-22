"use client";

import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import { BROADCAST_CHANNEL_NAME } from "@lib/constants/auth.constants";

export function usePermisoQuery(permiso: string) {
    return useQuery({
        queryKey: QUERY_KEYS.auth.permiso(permiso),
        enabled: permiso.trim().length > 0,
        queryFn: async () => {
            const response = await fetch(
                `${ROUTES.API_VALIDATE_PERMISSION}?permiso=${encodeURIComponent(permiso)}`,
                {
                    method: "GET",
                    cache: "no-store",
                    credentials: "same-origin",
                },
            );

            if (response.status === 401) {
                // Delegar la decisión de logout a TokenValidator para que aplique su backoff
                if (typeof window !== "undefined") {
                    const channel = new BroadcastChannel(BROADCAST_CHANNEL_NAME);
                    channel.postMessage({ type: "check" });
                    channel.close();
                }
                return false;
            }

            // 5xx → lanzar para que retry: 1 aplique (absorber transitorios)
            if (response.status >= 500) {
                throw new Error(`Transient error: ${response.status}`);
            }

            if (!response.ok) {
                return false;
            }

            const data = (await response.json()) as { tienePermiso?: boolean };
            return data.tienePermiso === true;
        },
        retry: 1,
        retryDelay: 1500,
        staleTime: 0,
    });
}
