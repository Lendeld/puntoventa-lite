"use client";

import { useProveedoresActivosQuery } from "@lib/hooks/useProveedoresActivosQuery";
import { Select, type SelectProps } from "@mantine/core";

type ProveedorSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage">;

export function ProveedorSelect(props: ProveedorSelectProps) {
    const { data } = useProveedoresActivosQuery();

    const options = (data ?? []).map((p) => ({ value: p.id, label: p.nombre }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={props.disabled}
            searchable
            clearable
            nothingFoundMessage="No hay proveedores activos"
            {...props}
        />
    );
}
