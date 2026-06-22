"use client";

import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerCodigosImpuestoService,
    obtenerCondicionesVentaService,
    obtenerMediosPagoService,
    obtenerTarifasIvaService,
    obtenerTiposIdentificacionService,
} from "@lib/services/configuracion.service";
import { Box, ScrollArea, Tabs } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { parseAsString, useQueryState } from "nuqs";
import { useMemo } from "react";
import { CodigosImpuestoList } from "@pages/sistema/catalogos/CodigosImpuestoList";
import { CondicionesVentaList } from "@pages/sistema/catalogos/CondicionesVentaList";
import { MediosPagoList } from "@pages/sistema/catalogos/MediosPagoList";
import { TarifasIvaList } from "@pages/sistema/catalogos/TarifasIvaList";
import { TiposIdentificacionList } from "@pages/sistema/catalogos/TiposIdentificacionList";

interface Props {
    puedeVerTipos: boolean;
    puedeToggleTipos: boolean;
    puedeVerCondicionesVenta: boolean;
    puedeToggleCondicionesVenta: boolean;
    puedeVerMediosPago: boolean;
    puedeToggleMediosPago: boolean;
    puedeVerCodigosImpuesto: boolean;
    puedeToggleCodigosImpuesto: boolean;
    puedeVerTarifasIva: boolean;
    puedeToggleTarifasIva: boolean;
}

export default function CatalogosPageSection({
    puedeVerTipos,
    puedeToggleTipos,
    puedeVerCondicionesVenta,
    puedeToggleCondicionesVenta,
    puedeVerMediosPago,
    puedeToggleMediosPago,
    puedeVerCodigosImpuesto,
    puedeToggleCodigosImpuesto,
    puedeVerTarifasIva,
    puedeToggleTarifasIva,
}: Props) {
    const tabs = useMemo(
        () =>
            [
                puedeVerTipos
                    ? { value: "tipos", label: "Tipo de identificación" }
                    : null,
                puedeVerCondicionesVenta
                    ? { value: "condiciones-venta", label: "Condiciones de venta" }
                    : null,
                puedeVerMediosPago
                    ? { value: "medios-pago", label: "Medios de pago" }
                    : null,
                puedeVerCodigosImpuesto
                    ? { value: "codigos-impuesto", label: "Códigos de impuesto" }
                    : null,
                puedeVerTarifasIva
                    ? { value: "tarifas-iva", label: "Tarifas IVA" }
                    : null,
            ].filter((tab): tab is { value: string; label: string } => !!tab),
        [
            puedeVerTipos,
            puedeVerCondicionesVenta,
            puedeVerMediosPago,
            puedeVerCodigosImpuesto,
            puedeVerTarifasIva,
        ],
    );
    // Tab sincronizado en la URL via nuqs. activeTab se deriva en render: si el
    // ?tab no es válido (o falta) cae al primer tab disponible.
    const [tabUrl, setTabUrl] = useQueryState(
        "tab",
        parseAsString.withDefault("").withOptions({ scroll: false }),
    );
    const activeTab = tabs.some((tab) => tab.value === tabUrl)
        ? tabUrl
        : (tabs[0]?.value ?? "tipos");

    const {
        data: tipos,
        isLoading: loadingTipos,
        isFetching: fetchingTipos,
        isError: isTiposError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.tiposIdentificacion(),
        enabled: puedeVerTipos,
        queryFn: async () => {
            const res = await obtenerTiposIdentificacionService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const {
        data: condicionesVenta,
        isLoading: loadingCondicionesVenta,
        isError: isCondicionesVentaError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.condicionesVenta(),
        enabled: puedeVerCondicionesVenta,
        queryFn: async () => {
            const res = await obtenerCondicionesVentaService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const {
        data: mediosPago,
        isLoading: loadingMediosPago,
        isError: isMediosPagoError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.mediosPago(),
        enabled: puedeVerMediosPago,
        queryFn: async () => {
            const res = await obtenerMediosPagoService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const {
        data: codigosImpuesto,
        isLoading: loadingCodigosImpuesto,
        isError: isCodigosImpuestoError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.codigosImpuesto(),
        enabled: puedeVerCodigosImpuesto,
        queryFn: async () => {
            const res = await obtenerCodigosImpuestoService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const {
        data: tarifasIva,
        isLoading: loadingTarifasIva,
        isError: isTarifasIvaError,
    } = useQuery({
        queryKey: QUERY_KEYS.configuracion.tarifasIva(),
        enabled: puedeVerTarifasIva,
        queryFn: async () => {
            const res = await obtenerTarifasIvaService();
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
                    if (value) setTabUrl(value);
                }}
                className="h-full"
            >
                <ScrollArea className="shrink-0 w-72 mr-12" scrollbarSize={6}>
                    <Tabs.List>
                        {tabs.map((tab) => (
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

                <Tabs.Panel value="tipos">
                    <TiposIdentificacionList
                        tipos={tipos}
                        loading={loadingTipos || fetchingTipos}
                        isError={isTiposError}
                        puedeToggleTipos={puedeToggleTipos}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="condiciones-venta">
                    <CondicionesVentaList
                        items={condicionesVenta}
                        loading={loadingCondicionesVenta}
                        isError={isCondicionesVentaError}
                        puedeToggle={puedeToggleCondicionesVenta}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="medios-pago">
                    <MediosPagoList
                        items={mediosPago}
                        loading={loadingMediosPago}
                        isError={isMediosPagoError}
                        puedeToggle={puedeToggleMediosPago}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="codigos-impuesto">
                    <CodigosImpuestoList
                        items={codigosImpuesto}
                        loading={loadingCodigosImpuesto}
                        isError={isCodigosImpuestoError}
                        puedeToggle={puedeToggleCodigosImpuesto}
                    />
                </Tabs.Panel>

                <Tabs.Panel value="tarifas-iva">
                    <TarifasIvaList
                        items={tarifasIva}
                        loading={loadingTarifasIva}
                        isError={isTarifasIvaError}
                        puedeToggle={puedeToggleTarifasIva}
                    />
                </Tabs.Panel>

            </Tabs>
        </Box>
    );
}
