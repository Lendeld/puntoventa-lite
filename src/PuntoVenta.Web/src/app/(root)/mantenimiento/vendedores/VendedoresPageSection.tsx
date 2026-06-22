"use client";

import { PermisoClient } from "@components/auth/PermisoClient";
import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import { StatusSegment } from "@components/ui/table/StatusSegment";
import { PERMISOS } from "@lib/constants/permisos.constants";
import {
    TABLE_ESTADO_DEFAULT,
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import { useVendedorQuery } from "@lib/hooks/useVendedorQuery";
import { ColumnDefinition, EstadoFiltro } from "@lib/types/base.types";
import type { VendedorDto } from "@lib/types/vendedores.types";
import { statusSegmentToActivo } from "@lib/utils/statuts.utils";
import { Badge, Box, Group, Text, Tooltip } from "@mantine/core";
import MenuVendedorAcciones from "@pages/mantenimiento/vendedores/MenuVendedorAcciones";
import NewVendedorSection from "@pages/mantenimiento/vendedores/NewVendedorSection";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";
import { DataTable } from "@ui/table/DataTable";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { TableHeader } from "@ui/table/TableHeader";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import { useState } from "react";

interface Props {
    puedeCrear: boolean;
    puedeEditar: boolean;
}

export default function VendedoresPageSection({
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

    const { data, isFetching, isError, refetch } = useVendedorQuery(params);

    const columns: ColumnDefinition<VendedorDto>[] = [
        {
            key: "nombre",
            header: "Vendedor",
            cell: (vendedor) => (
                <Group gap="xs">
                    <Text>{vendedor.nombre}</Text>
                    {vendedor.isPrincipal && (
                        <Tooltip label="Vendedor principal" position="top">
                            <Badge variant="light" color="blue">
                                Principal
                            </Badge>
                        </Tooltip>
                    )}
                    <EstadoInactivoBadge
                        activo={vendedor.activo}
                        estado={estado}
                    />
                </Group>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (vendedor) => (
                <AuditDateHoverCard
                    date={vendedor.fechaCreacion}
                    title="Creado"
                />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (vendedor) => (
                <AuditDateHoverCard
                    date={vendedor.fechaModificacion}
                    title="Modificado"
                />
            ),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (vendedor) => (
                <MenuVendedorAcciones id={vendedor.id} puedeEditar={puedeEditar} />
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
                            placeholder="Buscar vendedor..."
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
                        <PermisoClient permiso={PERMISOS.VENDEDORES_CREAR}>
                            <NewVendedorSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(vendedor) => vendedor.id}
                    getRowClassName={(vendedor) =>
                        vendedor.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay vendedores"
                    error={isError ? "Error al cargar los vendedores" : undefined}
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
