"use client";

import { useQuery } from "@tanstack/react-query";
import { obtenerResumenDashboardService } from "@lib/services/dashboard.service";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";

export function useDashboardResumenQuery() {
    return useQuery({
        queryKey: QUERY_KEYS.dashboardResumen,
        queryFn: async () => {
            const res = await obtenerResumenDashboardService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnWindowFocus: true,
    });
}
