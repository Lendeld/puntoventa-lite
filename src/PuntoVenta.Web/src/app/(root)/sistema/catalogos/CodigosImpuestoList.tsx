"use client";

import { toggleEstadoCodigoImpuestoAction } from "@lib/actions/configuracion.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { CodigoImpuestoDto } from "@lib/types/configuracion.types";
import { useQueryClient } from "@tanstack/react-query";
import { CatalogoList } from "@pages/sistema/catalogos/CatalogoList";

interface Props {
    items: CodigoImpuestoDto[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggle: boolean;
}

export function CodigosImpuestoList({ items, loading, isError, puedeToggle }: Props) {
    const queryClient = useQueryClient();

    async function handleSuccess() {
        await queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.configuracion.codigosImpuesto(),
        });
    }

    return (
        <CatalogoList
            title="Códigos de impuesto"
            description="Activa o desactiva códigos de impuesto disponibles para el sistema."
            errorMessage="Error al cargar códigos de impuesto."
            emptyMessage="No hay códigos de impuesto."
            items={items}
            loading={loading}
            isError={isError}
            puedeToggle={puedeToggle}
            onToggle={toggleEstadoCodigoImpuestoAction}
            onSuccess={handleSuccess}
        />
    );
}
