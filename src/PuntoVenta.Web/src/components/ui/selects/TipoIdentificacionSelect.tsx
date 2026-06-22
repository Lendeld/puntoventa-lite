"use client";

import { obtenerTiposIdentificacionService } from "@lib/services/configuracion.service";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { IconIdBadge2 } from "@tabler/icons-react";
import { Select, type SelectProps } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";

type StaticTipoItem = { value: string; label: string };

type TipoIdentificacionSelectProps = Omit<
    SelectProps,
    "data" | "leftSection" | "nothingFoundMessage"
> & {
    selectedValue?: string | null;
    staticData?: StaticTipoItem[];
};

export function TipoIdentificacionSelect({
    selectedValue,
    staticData,
    ...props
}: TipoIdentificacionSelectProps) {
    const tiposQuery = useQuery({
        queryKey: QUERY_KEYS.configuracion.tiposIdentificacion(),
        queryFn: async () => {
            const res = await obtenerTiposIdentificacionService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
        enabled: !staticData,
    });

    const data: StaticTipoItem[] = staticData
        ? staticData
        : (tiposQuery.data ?? []).map((tipo) => ({
              value: tipo.codigo,
              label: tipo.detalle,
              disabled: !tipo.activo && tipo.codigo !== selectedValue,
          }));

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            leftSection={<IconIdBadge2 size={16} />}
            data={data}
            nothingFoundMessage="No hay tipos de identificación"
            disabled={props.disabled}
            {...props}
        />
    );
}
