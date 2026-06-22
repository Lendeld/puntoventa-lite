"use client";

import { PermisoClient } from "@components/auth/PermisoClient";
import { Alert, Badge, Box, Group, Text } from "@mantine/core";
import { IconInfoCircle } from "@tabler/icons-react";
import { DataTable } from "@ui/table/DataTable";
import { TableHeader } from "@ui/table/TableHeader";
import { TableBody } from "@ui/table/TableBody";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import { useCajasQuery } from "@lib/hooks/useCajasQuery";
import { useNegocioQuery } from "@lib/hooks/useNegocioQuery";
import type { CajaListadoItemDto } from "@lib/types/cajas.types";
import type { ColumnDefinition } from "@lib/types/base.types";
import { PERMISOS } from "@lib/constants/permisos.constants";
import MenuCajaAcciones from "@pages/cajas/MenuCajaAcciones";
import NewCajaSection from "@pages/cajas/new/NewCajaSection";

interface Props {
    puedeCrear: boolean;
    puedeEditar: boolean;
    puedeToggle: boolean;
}

function estadoBadge(activo: boolean) {
    if (activo) {
        return (
            <Badge color="green" variant="light">
                Activa
            </Badge>
        );
    }
    return (
        <Badge color="red" variant="light">
            Inactiva
        </Badge>
    );
}

export default function CajasPageSection({ puedeCrear, puedeEditar, puedeToggle }: Props) {
    const { data, isFetching, isError, refetch } = useCajasQuery();
    const { data: negocio } = useNegocioQuery();
    const aplicaGestionCajas = negocio?.aplicaCajas ?? false;

    const columns: ColumnDefinition<CajaListadoItemDto>[] = [
        {
            key: "codigo",
            header: "Código",
            cell: (c) => (
                <Text fw={600}>{c.codigo}</Text>
            ),
        },
        {
            key: "nombre",
            header: "Nombre",
            cell: (c) => c.nombre,
        },
        {
            key: "estado",
            header: "Estado",
            cell: (c) => estadoBadge(c.activo),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (c) => (
                <MenuCajaAcciones
                    caja={c}
                    puedeEditar={puedeEditar}
                    puedeToggle={puedeToggle}
                />
            ),
        },
    ];

    return (
        <Box className="rounded-lg border border-theme-border-soft overflow-hidden h-page flex flex-col">
            {!aplicaGestionCajas && (
                <Alert
                    icon={<IconInfoCircle size={16} />}
                    color="blue"
                    variant="light"
                    radius={0}
                >
                    Gestión de cajas desactivada. Los documentos se emiten sin
                    asignar una caja específica.
                </Alert>
            )}
            <TableHeader>
                <Group justify="space-between" className="w-full">
                    <Group>
                        <TableRefreshButton
                            onClick={() => refetch()}
                            loading={isFetching}
                        />
                    </Group>
                    {puedeCrear && (
                        <PermisoClient permiso={PERMISOS.CAJAS_CREAR}>
                            <NewCajaSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data ?? []}
                    loading={isFetching}
                    getRowId={(c) => c.id}
                    getRowClassName={(c) => (c.activo ? undefined : "opacity-60")}
                    emptyText="No hay cajas registradas"
                    error={isError ? "Error al cargar las cajas" : undefined}
                />
            </TableBody>
        </Box>
    );
}
