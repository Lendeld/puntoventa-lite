"use client";

import { toggleEstadoTipoIdentificacionAction } from "@lib/actions/configuracion.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import type { TipoIdentificacionDto } from "@lib/types/configuracion.types";
import { useQueryClient } from "@tanstack/react-query";
import { CatalogoList } from "@pages/sistema/catalogos/CatalogoList";

interface TiposIdentificacionListProps {
    tipos: TipoIdentificacionDto[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggleTipos: boolean;
}

export function TiposIdentificacionList({
    tipos,
    loading,
    isError,
    puedeToggleTipos,
}: TiposIdentificacionListProps) {
    const queryClient = useQueryClient();

    async function handleSuccess() {
        await queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.configuracion.tiposIdentificacion(),
        });
    }

    return (
        <CatalogoList
            title="Tipos de identificación"
            description="Activa o desactiva tipos disponibles para el sistema."
            errorMessage="Error al cargar tipos de identificación."
            emptyMessage="No hay tipos de identificación."
            items={tipos}
            loading={loading}
            isError={isError}
            puedeToggle={puedeToggleTipos}
            onToggle={toggleEstadoTipoIdentificacionAction}
            onSuccess={handleSuccess}
        />
    );
}
