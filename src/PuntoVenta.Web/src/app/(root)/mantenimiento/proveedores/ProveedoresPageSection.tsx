"use client";

import { useState } from "react";
import { Box, Group, Stack, Text } from "@mantine/core";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { useProveedorQuery } from "@lib/hooks/useProveedorQuery";
import type { ProveedorDto } from "@lib/types/proveedores.types";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableHeader } from "@ui/table/TableHeader";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import {
    TABLE_ESTADO_DEFAULT,
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import { ColumnDefinition, EstadoFiltro } from "@lib/types/base.types";
import { statusSegmentToActivo } from "@lib/utils/statuts.utils";
import { StatusSegment } from "@components/ui/table/StatusSegment";
import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import { PermisoClient } from "@components/auth/PermisoClient";
import { DataTable } from "@ui/table/DataTable";
import NewProveedorSection from "@pages/mantenimiento/proveedores/NewProveedorSection";
import MenuProveedorAcciones from "@pages/mantenimiento/proveedores/MenuProveedorAcciones";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";

interface Props {
    puedeCrear: boolean;
    puedeEditar: boolean;
}

export default function ProveedoresPageSection({
    puedeCrear,
    puedeEditar,
}: Props) {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(TABLE_PAGE_SIZE_DEFAULT);
    const [search, setSearch] = useState("");
    const [estado, setEstado] = useState<EstadoFiltro>(TABLE_ESTADO_DEFAULT);

    const params = {
        numeroPagina: page,
        tamanoPagina: pageSize,
        filtroDinamico: search || undefined,
        activo: statusSegmentToActivo(estado),
    };

    const { data, isFetching, isError, refetch } = useProveedorQuery(params);

    const columns: ColumnDefinition<ProveedorDto>[] = [
        {
            key: "nombre",
            header: "Nombre",
            cell: (proveedor) => (
                <Stack gap={4}>
                    <Group gap="xs" wrap="nowrap">
                        <Text>{proveedor.nombre}</Text>
                        <EstadoInactivoBadge
                            activo={proveedor.activo}
                            estado={estado}
                        />
                    </Group>
                    <Text size="sm" className="text-theme-text-muted">
                        {proveedor.correo ?? proveedor.telefono ?? "Sin contacto"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (proveedor) => (
                <AuditDateHoverCard
                    date={proveedor.fechaCreacion}
                    title="Creado"
                />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (proveedor) => (
                <AuditDateHoverCard
                    date={proveedor.fechaModificacion}
                    title="Modificado"
                />
            ),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (proveedor) => (
                <MenuProveedorAcciones
                    id={proveedor.id}
                    puedeEditar={puedeEditar}
                />
            ),
        },
    ];

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
                            placeholder="Buscar proveedor..."
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
                    {puedeCrear && (
                        <PermisoClient permiso={PERMISOS.PROVEEDORES_CREAR}>
                            <NewProveedorSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(proveedor) => proveedor.id}
                    getRowClassName={(proveedor) =>
                        proveedor.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay proveedores"
                    error={
                        isError ? "Error al cargar los proveedores" : undefined
                    }
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
