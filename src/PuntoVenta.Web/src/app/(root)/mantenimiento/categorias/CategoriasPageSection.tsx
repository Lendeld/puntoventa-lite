"use client";

import { useState } from "react";
import { Box, Group, Stack, Text } from "@mantine/core";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { useCategoriaQuery } from "@lib/hooks/useCategoriaQuery";
import type { CategoriaDto } from "@lib/types/categorias.types";
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
import NewCategoriaSection from "@pages/mantenimiento/categorias/NewCategoriaSection";
import MenuCategoriaAcciones from "@pages/mantenimiento/categorias/MenuCategoriaAcciones";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";

interface Props {
    puedeCrear: boolean;
    puedeEditar: boolean;
}

export default function CategoriasPageSection({
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

    const { data, isFetching, isError, refetch } = useCategoriaQuery(params);

    const columns: ColumnDefinition<CategoriaDto>[] = [
        {
            key: "nombre",
            header: "Nombre",
            cell: (categoria) => (
                <Stack gap={4}>
                    <Group gap="xs" wrap="nowrap">
                        <Text>{categoria.nombre}</Text>
                        <EstadoInactivoBadge
                            activo={categoria.activo}
                            estado={estado}
                        />
                    </Group>
                    <Text size="sm" className="text-theme-text-muted">
                        {categoria.descripcion ?? "Sin descripción"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (categoria) => (
                <AuditDateHoverCard
                    date={categoria.fechaCreacion}
                    title="Creado"
                />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (categoria) => (
                <AuditDateHoverCard
                    date={categoria.fechaModificacion}
                    title="Modificado"
                />
            ),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (categoria) => (
                <MenuCategoriaAcciones
                    id={categoria.id}
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
                            placeholder="Buscar categoría..."
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
                        <PermisoClient permiso={PERMISOS.CATEGORIAS_CREAR}>
                            <NewCategoriaSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(categoria) => categoria.id}
                    getRowClassName={(categoria) =>
                        categoria.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay categorías"
                    error={
                        isError ? "Error al cargar las categorías" : undefined
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
