"use client";

import { useRolesActivosQuery } from "@lib/hooks/useRolesActivosQuery";
import { IconShield } from "@tabler/icons-react";
import { Select, type SelectProps } from "@mantine/core";

type RoleSelectProps = Omit<
    SelectProps,
    "data" | "leftSection" | "nothingFoundMessage"
>;

export function RoleSelect(props: RoleSelectProps) {
    const rolesQuery = useRolesActivosQuery();

    const data = (rolesQuery.data ?? []).map((rol) => ({
        value: rol.id,
        label: rol.nombre,
    }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            leftSection={<IconShield size={16} />}
            data={data}
            disabled={props.disabled}
            nothingFoundMessage="No hay roles activos"
            {...props}
        />
    );
}
