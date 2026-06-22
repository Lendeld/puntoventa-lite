"use client";

import { useState } from "react";
import { Badge, Box, Group, Stack, Text } from "@mantine/core";
import { DataTable } from "@ui/table/DataTable";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableHeader } from "@ui/table/TableHeader";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { useRolesQuery } from "@lib/hooks/useRolesQuery";
import {
    TABLE_ESTADO_DEFAULT,
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import type { RolDto } from "@lib/types/roles.types";
import { ColumnDefinition, EstadoFiltro } from "@lib/types/base.types";
import { statusSegmentToActivo } from "@lib/utils/statuts.utils";
import { StatusSegment } from "@components/ui/table/StatusSegment";
import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import NewRolSection from "@pages/roles/new/NewRolSection";
import MenuRolAcciones from "@pages/roles/menu/MenuAcciones";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";
import { PermisoClient } from "@components/auth/PermisoClient";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { useDeploymentMode } from "@lib/hooks/useDeploymentMode";

function buildColumns(
    mostrarAcciones: boolean,
    estado: EstadoFiltro,
): ColumnDefinition<RolDto>[] {
    const cols: ColumnDefinition<RolDto>[] = [
        {
            key: "nombre",
            header: "Nombre",
            cell: (rol) => (
                <Stack gap={4}>
                    <Group>
                        <Text>{rol.nombre}</Text>
                        {rol.isPrincipal && (
                            <Badge color="blue" variant="light">
                                Principal
                            </Badge>
                        )}
                        <EstadoInactivoBadge
                            activo={rol.activo}
                            estado={estado}
                        />
                    </Group>

                    <Text size="sm" className="text-theme-text-muted">
                        {rol.descripcion ?? "Sin descripción"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (rol) => (
                <AuditDateHoverCard date={rol.fechaCreacion} title="Creado por" />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (rol) => (
                <AuditDateHoverCard
                    date={rol.fechaModificacion}
                    title="Modificado por"
                />
            ),
        },
    ];

    if (mostrarAcciones) {
        cols.push({
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (rol) => (
                <MenuRolAcciones id={rol.id} isPrincipal={rol.isPrincipal} />
            ),
        });
    }

    return cols;
}

export default function RolesPageSection() {
    const { esLocalHost } = useDeploymentMode();
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(TABLE_PAGE_SIZE_DEFAULT);
    const [search, setSearch] = useState("");
    const [estado, setEstado] = useState<EstadoFiltro>(TABLE_ESTADO_DEFAULT);
    const columns = buildColumns(!esLocalHost, estado);

    const params = {
        numeroPagina: page,
        tamanoPagina: pageSize,
        filtroDinamico: search || undefined,
        activo: statusSegmentToActivo(estado),
    };

    const { data, isFetching, isError, refetch } = useRolesQuery(params);

    function handleSearchChange(value: string) {
        setPage(1);
        setSearch(value);
    }

    function handleEstadoChange(value: EstadoFiltro) {
        setPage(1);
        setEstado(value);
    }

    function handlePageSizeChange(size: number) {
        setPage(1);
        setPageSize(size);
    }

    return (
        <Box className="rounded-lg border border-theme-border-soft overflow-hidden h-page flex flex-col">
            <TableHeader>
                <Group justify="space-between" className="w-full">
                    <Group>
                        <DynamicSearchInput
                            value={search}
                            onChange={handleSearchChange}
                            placeholder="Buscar rol..."
                            className="min-w-100 w-120"
                        />
                        <StatusSegment
                            value={estado}
                            onChange={handleEstadoChange}
                        />
                        <TableRefreshButton
                            onClick={() => refetch()}
                            loading={isFetching}
                        />
                    </Group>
                    {!esLocalHost && (
                        <PermisoClient permiso={PERMISOS.ROLES_CREAR}>
                            <NewRolSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(rol) => rol.id}
                    getRowClassName={(rol) =>
                        rol.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay roles"
                    error={isError ? "Error al cargar los roles" : undefined}
                />
            </TableBody>
            <TableFooter>
                <TablePagination
                    page={data?.pagina ?? page}
                    pageSize={data?.tamano ?? pageSize}
                    total={data?.totalRegistros ?? 0}
                    totalPages={data?.totalPaginas ?? 1}
                    onPageChange={setPage}
                    onPageSizeChange={handlePageSizeChange}
                    pageSizeOptions={TABLE_PAGE_SIZE_OPTIONS}
                />
            </TableFooter>
        </Box>
    );
}
