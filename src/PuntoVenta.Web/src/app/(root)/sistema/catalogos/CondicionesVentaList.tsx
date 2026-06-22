"use client";

import { toggleEstadoCondicionVentaAction } from "@lib/actions/configuracion.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { CondicionVentaDto } from "@lib/types/configuracion.types";
import { useQueryClient } from "@tanstack/react-query";
import { CatalogoList } from "@pages/sistema/catalogos/CatalogoList";

interface Props {
    items: CondicionVentaDto[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggle: boolean;
}

export function CondicionesVentaList({ items, loading, isError, puedeToggle }: Props) {
    const queryClient = useQueryClient();

    async function handleSuccess() {
        await queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.configuracion.condicionesVenta(),
        });
    }

    return (
        <CatalogoList
            title="Condiciones de venta"
            description="Activa o desactiva condiciones de venta disponibles para el sistema."
            errorMessage="Error al cargar condiciones de venta."
            emptyMessage="No hay condiciones de venta."
            items={items}
            loading={loading}
            isError={isError}
            puedeToggle={puedeToggle}
            onToggle={toggleEstadoCondicionVentaAction}
            onSuccess={handleSuccess}
        />
    );
}
