"use client";

import { Badge } from "@mantine/core";
import type { EstadoFiltro } from "@lib/types/base.types";

interface Props {
    activo: boolean;
    estado: EstadoFiltro;
}

export function EstadoInactivoBadge({ activo, estado }: Props) {
    if (activo) return null;
    if (estado !== "todos") return null;
    return (
        <Badge color="red" variant="light" size="sm" radius="sm">
            Inactivo
        </Badge>
    );
}
