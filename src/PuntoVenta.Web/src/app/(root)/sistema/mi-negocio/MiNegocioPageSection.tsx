"use client";

import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerNegocioService } from "@lib/services/configuracion.service";
import { Box, ScrollArea, Tabs } from "@mantine/core";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { parseAsStringLiteral, useQueryState } from "nuqs";
import { ConfiguracionAvanzadaPanel } from "@pages/sistema/mi-negocio/ConfiguracionAvanzadaPanel";
import { NegocioPanel } from "@pages/sistema/mi-negocio/NegocioPanel";
import { TicketsImpresionPanel } from "@pages/sistema/mi-negocio/TicketsImpresionPanel";

interface Props {
    puedeEditarNegocio: boolean;
}

const TABS = [
    { value: "negocio", label: "Negocio" },
    { value: "tickets-impresion", label: "Tickets e impresión" },
    { value: "configuracion-avanzada", label: "Configuración avanzada" },
] as const;

const TAB_VALUES = TABS.map((t) => t.value);

export default function MiNegocioPageSection({ puedeEditarNegocio }: Props) {
    const queryClient = useQueryClient();

    const [activeTab, setActiveTab] = useQueryState(
        "tab",
        parseAsStringLiteral(TAB_VALUES).withDefault("negocio"),
    );

    const {
        data: negocio,
        isLoading: loadingNegocio,
        isError: isNegocioError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.negocio,
        queryFn: async () => {
            const res = await obtenerNegocioService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    return (
        <Box className="h-page pt-1 px-1">
            <Tabs
                orientation="vertical"
                value={activeTab}
                onChange={(value) => {
                    if (value) setActiveTab(value as (typeof TAB_VALUES)[number]);
                }}
                className="h-full"
            >
                <ScrollArea className="shrink-0 w-72 mr-12" scrollbarSize={6}>
                    <Tabs.List>
                        {TABS.map((tab) => (
                            <Tabs.Tab
                                key={tab.value}
                                value={tab.value}
                                className="font-bold"
                            >
                                {tab.label}
                            </Tabs.Tab>
                        ))}
                    </Tabs.List>
                </ScrollArea>

                <Tabs.Panel value="negocio" className="min-h-0 flex-1">
                    <NegocioPanel
                        puedeEditarNegocio={puedeEditarNegocio}
                        negocio={negocio}
                        loadingNegocio={loadingNegocio}
                        isNegocioError={isNegocioError}
                        onSaveSuccess={async () => {
                            await queryClient.invalidateQueries({
                                queryKey: QUERY_KEYS.configuracion.negocio,
                            });
                        }}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="configuracion-avanzada" className="min-h-0 flex-1">
                    <ConfiguracionAvanzadaPanel
                        puedeEditarNegocio={puedeEditarNegocio}
                        negocio={negocio}
                        loadingNegocio={loadingNegocio}
                        isNegocioError={isNegocioError}
                        onSaveSuccess={async () => {
                            await queryClient.invalidateQueries({
                                queryKey: QUERY_KEYS.configuracion.negocio,
                            });
                        }}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="tickets-impresion" className="min-h-0 flex-1">
                    <TicketsImpresionPanel
                        puedeVerPlantilla={true}
                        puedeEditarPlantilla={puedeEditarNegocio}
                    />
                </Tabs.Panel>
            </Tabs>
        </Box>
    );
}
