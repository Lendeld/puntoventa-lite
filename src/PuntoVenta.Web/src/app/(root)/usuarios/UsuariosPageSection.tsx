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
import { useUsuariosQuery } from "@lib/hooks/useUsuariosQuery";
import {
    TABLE_ESTADO_DEFAULT,
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import type { UsuarioDto } from "@lib/types/usuarios.types";
import { ColumnDefinition, EstadoFiltro } from "@lib/types/base.types";
import { statusSegmentToActivo } from "@lib/utils/statuts.utils";
import { StatusSegment } from "@components/ui/table/StatusSegment";
import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import MenuUsuarioAcciones from "@pages/usuarios/menu/MenuAcciones";
import { EstadoInactivoBadge } from "@ui/table/EstadoInactivoBadge";
import NewUsuarioSection from "@pages/usuarios/new/NewUsuarioSection";

interface Props {
    puedeEditarUsuarios: boolean;
    puedeCrearUsuarios: boolean;
}

export default function UsuariosPageSection({
    puedeEditarUsuarios,
    puedeCrearUsuarios,
}: Props) {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(TABLE_PAGE_SIZE_DEFAULT);
    const [search, setSearch] = useState("");
    const [estado, setEstado] = useState<EstadoFiltro>(TABLE_ESTADO_DEFAULT);

    const columns: ColumnDefinition<UsuarioDto>[] = [
        {
            key: "nombreUsuario",
            header: "Usuario",
            cell: (u) => (
                <Group gap="xs" wrap="nowrap">
                    <Text>{u.nombreUsuario}</Text>
                    <EstadoInactivoBadge activo={u.activo} estado={estado} />
                </Group>
            ),
        },
        {
            key: "nombre",
            header: "Nombre",
            cell: (u) => (
                <Stack gap={4}>
                    <Text size="sm" className="text-theme-text-muted">
                        {u.identificacion}
                    </Text>
                    <Text>{u.nombre}</Text>
                </Stack>
            ),
        },
        {
            key: "rolNombre",
            header: "Rol",
            cell: (u) => (
                <Group gap={6}>
                    <Text size="sm">{u.rolNombre ?? "Sin rol"}</Text>
                    {u.esPropietario && (
                        <Badge size="xs" variant="light" color="blue">
                            Propietario
                        </Badge>
                    )}
                </Group>
            ),
        },
        { key: "correo", header: "Correo", cell: (u) => u.correo ?? "—" },
        { key: "telefono", header: "Teléfono", cell: (u) => u.telefono ?? "—" },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (u) => (
                <AuditDateHoverCard
                    date={u.fechaCreacion}
                    user={u.creadoPor}
                    title="Creado por "
                />
            ),
        },
        {
            key: "fechaModificacion",
            header: "Modificación",
            cell: (u) => (
                <AuditDateHoverCard
                    date={u.fechaModificacion}
                    user={u.modificadoPor}
                    title="Modificado por "
                />
            ),
        },
        ...(puedeEditarUsuarios
            ? [
                  {
                      key: "acciones",
                      header: "Acciones",
                      align: "center" as const,
                      cell: (u: UsuarioDto) => (
                          <MenuUsuarioAcciones id={u.id} />
                      ),
                  },
              ]
            : []),
    ];

    const params = {
        numeroPagina: page,
        tamanoPagina: pageSize,
        filtroDinamico: search || undefined,
        activo: statusSegmentToActivo(estado),
    };

    const { data, isFetching, isError, refetch } = useUsuariosQuery(params);

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
                            placeholder="Buscar usuario..."
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
                    {puedeCrearUsuarios && <NewUsuarioSection />}
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(u) => u.id}
                    getRowClassName={(u) =>
                        u.activo ? undefined : "opacity-60"
                    }
                    emptyText="No hay usuarios"
                    error={isError ? "Error al cargar los usuarios" : undefined}
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
