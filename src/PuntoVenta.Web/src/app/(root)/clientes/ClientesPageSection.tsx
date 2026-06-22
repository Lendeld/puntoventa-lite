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
import { useClienteQuery } from "@lib/hooks/useClienteQuery";
import { ColumnDefinition, EstadoFiltro } from "@lib/types/base.types";
import type { ClienteListaDto } from "@lib/types/clientes.types";
import { statusSegmentToActivo } from "@lib/utils/statuts.utils";
import { Box, Group, Stack, Text } from "@mantine/core";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";
import MenuClienteAcciones from "@pages/clientes/MenuClienteAcciones";
import NewClienteSection from "@pages/clientes/NewClienteSection";
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

export default function ClientesPageSection({ puedeCrear, puedeEditar }: Props) {
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

    const { data, isFetching, isError, refetch } = useClienteQuery(params);

    const columns: ColumnDefinition<ClienteListaDto>[] = [
        {
            key: "nombre",
            header: "Cliente",
            cell: (cliente) => (
                <Stack gap={4}>
                    <Group gap="xs" wrap="nowrap">
                        <Text>{cliente.nombre}</Text>
                        <EstadoInactivoBadge
                            activo={cliente.activo}
                            estado={estado}
                        />
                    </Group>
                    <Text size="sm" className="text-theme-text-muted">
                        {cliente.identificacion ?? "Sin identificación"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "contacto",
            header: "Contacto",
            cell: (cliente) => (
                <Stack gap={4}>
                    <Text size="sm">{cliente.correo ?? "Sin correo"}</Text>
                    <Text size="sm" className="text-theme-text-muted">
                        {cliente.telefono ?? "Sin teléfono"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (cliente) => (
                <AuditDateHoverCard
                    date={cliente.fechaCreacion}
                    title="Creado"
                />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (cliente) => (
                <AuditDateHoverCard
                    date={cliente.fechaModificacion}
                    title="Modificado"
                />
            ),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (cliente) => (
                <MenuClienteAcciones id={cliente.id} puedeEditar={puedeEditar} />
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
                            placeholder="Buscar cliente..."
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
                        <PermisoClient permiso={PERMISOS.CLIENTES_CREAR}>
                            <NewClienteSection />
                        </PermisoClient>
                    )}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(cliente) => cliente.id}
                    getRowClassName={(cliente) =>
                        cliente.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay clientes"
                    error={isError ? "Error al cargar los clientes" : undefined}
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
