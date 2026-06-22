"use client";

import { useCodigosImpuestoActivosQuery } from "@lib/hooks/useCodigosImpuestoActivosQuery";
import { Select, type SelectProps } from "@mantine/core";

type CodigoImpuestoSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage">;

export function CodigoImpuestoSelect(props: CodigoImpuestoSelectProps) {
    const { data } = useCodigosImpuestoActivosQuery();

    const options = (data ?? []).map((c) => ({
        value: c.codigo,
        label: c.detalle,
    }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={props.disabled}
            searchable
            allowDeselect={false}
            nothingFoundMessage="No hay códigos de impuesto activos"
            {...props}
        />
    );
}
