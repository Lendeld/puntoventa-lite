"use client";

import { toggleEstadoTarifaIvaAction } from "@lib/actions/configuracion.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { TarifaIvaImpuestoDto } from "@lib/types/configuracion.types";
import { Badge } from "@mantine/core";
import { useQueryClient } from "@tanstack/react-query";
import { CatalogoList } from "@pages/sistema/catalogos/CatalogoList";

interface Props {
    items: TarifaIvaImpuestoDto[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggle: boolean;
}

export function TarifasIvaList({ items, loading, isError, puedeToggle }: Props) {
    const queryClient = useQueryClient();

    async function handleSuccess() {
        await queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.configuracion.tarifasIva(),
        });
    }

    return (
        <CatalogoList
            title="Tarifas IVA"
            description="Activa o desactiva tarifas IVA disponibles para el sistema."
            errorMessage="Error al cargar tarifas IVA."
            emptyMessage="No hay tarifas IVA."
            items={items}
            loading={loading}
            isError={isError}
            puedeToggle={puedeToggle}
            onToggle={toggleEstadoTarifaIvaAction}
            onSuccess={handleSuccess}
            renderExtraBadges={(item) => (
                <Badge variant="filled" color="accentPV" size="sm">
                    {item.porcentaje}%
                </Badge>
            )}
        />
    );
}
