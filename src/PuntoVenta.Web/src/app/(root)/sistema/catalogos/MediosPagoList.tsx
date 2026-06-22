"use client";

import { toggleEstadoMedioPagoAction } from "@lib/actions/configuracion.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { MedioPagoDto } from "@lib/types/configuracion.types";
import { useQueryClient } from "@tanstack/react-query";
import { CatalogoList } from "@pages/sistema/catalogos/CatalogoList";

interface Props {
    items: MedioPagoDto[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggle: boolean;
}

export function MediosPagoList({ items, loading, isError, puedeToggle }: Props) {
    const queryClient = useQueryClient();

    async function handleSuccess() {
        await queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.configuracion.mediosPago(),
        });
    }

    return (
        <CatalogoList
            title="Medios de pago"
            description="Activa o desactiva medios de pago disponibles para el sistema."
            errorMessage="Error al cargar medios de pago."
            emptyMessage="No hay medios de pago."
            items={items}
            loading={loading}
            isError={isError}
            puedeToggle={puedeToggle}
            onToggle={toggleEstadoMedioPagoAction}
            onSuccess={handleSuccess}
        />
    );
}
