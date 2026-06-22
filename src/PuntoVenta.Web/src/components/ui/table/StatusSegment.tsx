"use client";

import { Select } from "@mantine/core";
import { TABLE_ESTADO_DEFAULT } from "@lib/constants/table.constants";
import { EstadoFiltro } from "@/lib/types/base.types";

interface Props {
    value: EstadoFiltro;
    onChange: (value: EstadoFiltro) => void;
}

const OPTIONS = [
    { label: "Activos", value: "activos" },
    { label: "Inactivos", value: "inactivos" },
    { label: "Todos", value: "todos" },
];

export function StatusSegment({ value, onChange }: Props) {
    return (
        <Select
            data={OPTIONS}
            value={value}
            onChange={(v) => onChange((v ?? TABLE_ESTADO_DEFAULT) as EstadoFiltro)}
            defaultValue={TABLE_ESTADO_DEFAULT}
            size="sm"
            w={140}
            allowDeselect={false}
        />
    );
}
