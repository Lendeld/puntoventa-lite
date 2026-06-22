"use client";

import { useCategoriasActivasQuery } from "@lib/hooks/useCategoriasActivasQuery";
import { Select, type SelectProps } from "@mantine/core";

type CategoriaSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage">;

export function CategoriaSelect(props: CategoriaSelectProps) {
    const { data } = useCategoriasActivasQuery();

    const options = (data ?? []).map((c) => ({ value: c.id, label: c.nombre }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={props.disabled}
            searchable
            allowDeselect={false}
            nothingFoundMessage="No hay categorías activas"
            {...props}
        />
    );
}
