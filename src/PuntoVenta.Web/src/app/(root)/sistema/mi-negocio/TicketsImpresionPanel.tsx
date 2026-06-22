"use client";

import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerNegocioTicketConfigService,
    obtenerPerfilesImpresoraService,
} from "@lib/services/impresion.service";
import { ScrollArea, Stack } from "@mantine/core";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { EstaComputadoraSection } from "@pages/sistema/mi-negocio/ticket/EstaComputadoraSection";
import { PlantillaTicketSection } from "@pages/sistema/mi-negocio/ticket/PlantillaTicketSection";

interface Props {
    puedeVerPlantilla: boolean;
    puedeEditarPlantilla: boolean;
}

export function TicketsImpresionPanel({
    puedeVerPlantilla,
    puedeEditarPlantilla,
}: Props) {
    const queryClient = useQueryClient();

    const {
        data: config,
        isLoading: loadingConfig,
        isError: isConfigError,
    } = useQuery({
        queryKey: QUERY_KEYS.impresion.ticketConfig,
        enabled: puedeVerPlantilla,
        queryFn: async () => {
            const res = await obtenerNegocioTicketConfigService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const {
        data: perfiles,
        isLoading: perfilesLoading,
        isError: perfilesError,
    } = useQuery({
        queryKey: QUERY_KEYS.impresion.perfiles,
        queryFn: async () => {
            const res = await obtenerPerfilesImpresoraService();
            if (res.errors) throw res.errors;
            return res.data ?? [];
        },
        staleTime: 60_000,
    });

    return (
        <ScrollArea className="h-full min-h-0" scrollbarSize={6}>
            <Stack gap="lg" p="md">
                <EstaComputadoraSection
                    perfiles={perfiles}
                    perfilesLoading={perfilesLoading}
                    perfilesError={perfilesError}
                />

                {puedeVerPlantilla && (
                    <PlantillaTicketSection
                        config={config}
                        loading={loadingConfig}
                        isError={isConfigError}
                        puedeEditar={puedeEditarPlantilla}
                        onSaveSuccess={async () => {
                            await queryClient.invalidateQueries({
                                queryKey: QUERY_KEYS.impresion.ticketConfig,
                            });
                        }}
                    />
                )}
            </Stack>
        </ScrollArea>
    );
}
