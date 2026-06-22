"use client";

import { useTarifasIvaActivasQuery } from "@lib/hooks/useTarifasIvaActivasQuery";
import { Select, type SelectProps } from "@mantine/core";

type TarifaIvaSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage">;

export function TarifaIvaSelect(props: TarifaIvaSelectProps) {
    const { data } = useTarifasIvaActivasQuery();

    const options = (data ?? []).map((t) => ({
        value: t.codigo,
        label: `${t.detalle} (${t.porcentaje}%)`,
    }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={props.disabled}
            searchable
            nothingFoundMessage="No hay tarifas IVA activas"
            {...props}
        />
    );
}
