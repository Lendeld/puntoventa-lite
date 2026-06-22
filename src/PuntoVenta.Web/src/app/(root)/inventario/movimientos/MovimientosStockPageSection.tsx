"use client";

import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import { TABLE_PAGE_SIZE_DEFAULT, TABLE_PAGE_SIZE_OPTIONS } from "@lib/constants/table.constants";
import { useMovimientosStockQuery } from "@lib/hooks/useMovimientosStockQuery";
import { useProductosQuery } from "@lib/hooks/useProductosQuery";
import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import { ColumnDefinition } from "@lib/types/base.types";
import type { MovimientoStockDto } from "@lib/types/inventario.types";
import {
    Badge,
    Box,
    Group,
    Select,
    Stack,
    Text,
    Tooltip,
} from "@mantine/core";
import { DataTable } from "@ui/table/DataTable";
import { useMemo } from "react";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { TableHeader } from "@ui/table/TableHeader";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";

const TIPO_DOCUMENTO_LABELS: Record<string, string> = {
    Factura: "Factura",
    NotaCredito: "Nota crédito",
    NotaDebito: "Nota débito",
    Proforma: "Proforma",
    Apartado: "Apartado",
};

function getTipoDocLabel(tipo: string | null): string {
    if (!tipo) return "Ajuste manual";
    return TIPO_DOCUMENTO_LABELS[tipo] ?? tipo;
}

// react-doctor-disable-next-line react-doctor/no-giant-component
export default function MovimientosStockPageSection() {
    const [{ page, pageSize, productoFiltro }, patchState] = usePatchReducer({
        page: 1,
        pageSize: TABLE_PAGE_SIZE_DEFAULT,
        productoFiltro: null as string | null,
    });

    const params = {
        productoId: productoFiltro ?? undefined,
        pagina: page,
        tamano: pageSize,
    };

    const { data, isFetching, isError, refetch } = useMovimientosStockQuery(params);

    const { data: productosData } = useProductosQuery({
        numeroPagina: 1,
        tamanoPagina: 200,
    });

    const productosOptions = useMemo(
        () => [
            { value: "", label: "Todos los productos" },
            ...(productosData?.items ?? []).map((p) => ({ value: p.id, label: p.nombre })),
        ],
        [productosData],
    );

    const columns: ColumnDefinition<MovimientoStockDto>[] = [
        {
            key: "fechaUtc",
            header: "Fecha",
            cell: (m) => (
                <AuditDateHoverCard date={m.fechaUtc} title="Registrado" />
            ),
        },
        {
            key: "productoId",
            header: "Producto",
            cell: (m) => <Text size="sm">{m.nombreProducto}</Text>,
        },
        {
            key: "tipoDocumentoOrigen",
            header: "Origen",
            cell: (m) => (
                <Stack gap={2}>
                    <Text size="sm">{getTipoDocLabel(m.tipoDocumentoOrigen)}</Text>
                    {m.consecutivoDocumento && (
                        <Text size="xs" c="dimmed" ff="monospace">
                            {m.consecutivoDocumento}
                        </Text>
                    )}
                </Stack>
            ),
        },
        {
            key: "delta",
            header: "Cantidad",
            align: "right",
            cell: (m) => (
                <Text
                    size="sm"
                    fw={600}
                    c={m.delta > 0 ? "green" : "red"}
                    className="tabular-nums"
                >
                    {m.delta > 0 ? "+" : ""}
                    {m.delta.toLocaleString("es-CR", {
                        minimumFractionDigits: 0,
                        maximumFractionDigits: 5,
                    })}
                </Text>
            ),
        },
        {
            key: "saldoResultante",
            header: "Saldo",
            align: "right",
            cell: (m) => (
                <Text size="sm" className="tabular-nums">
                    {m.saldoResultante.toLocaleString("es-CR", {
                        minimumFractionDigits: 0,
                        maximumFractionDigits: 5,
                    })}
                </Text>
            ),
        },
        {
            key: "razon",
            header: "Razón / Nota",
            cell: (m) => {
                if (!m.razon) return <Text size="sm" c="dimmed">—</Text>;
                return (
                    <Tooltip label={m.razon} multiline w={240} withArrow>
                        <Text size="sm" lineClamp={2} style={{ cursor: "default" }}>
                            {m.razon}
                        </Text>
                    </Tooltip>
                );
            },
        },
    ];

    const total = data?.total ?? 0;
    const items = data?.items ?? [];

    // Derivar paginación manual desde total / tamano
    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    return (
        <Box className="rounded-lg border border-theme-border-soft bg-theme-surface overflow-hidden h-page flex flex-col">
            <TableHeader>
                <Group justify="space-between" className="w-full" wrap="nowrap">
                    <Group gap="sm" wrap="wrap">
                        <Select
                            placeholder="Filtrar por producto"
                            data={productosOptions}
                            value={productoFiltro ?? ""}
                            onChange={(val) =>
                                patchState({ page: 1, productoFiltro: val || null })
                            }
                            searchable
                            allowDeselect={false}
                            clearable
                            className="min-w-60 w-80"
                            size="sm"
                        />
                        <TableRefreshButton onClick={() => refetch()} loading={isFetching} />
                    </Group>
                    <Group gap="xs">
                        <Badge variant="light" color="gray" size="sm">
                            {total} movimiento{total !== 1 ? "s" : ""}
                        </Badge>
                    </Group>
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={items}
                    loading={isFetching}
                    getRowId={(m) => m.id}
                    emptyText="No hay movimientos de stock"
                    error={isError ? "Error al cargar los movimientos" : undefined}
                />
            </TableBody>
            <TableFooter>
                <TablePagination
                    page={page}
                    pageSize={pageSize}
                    total={total}
                    totalPages={totalPages}
                    onPageChange={(p) => patchState({ page: p })}
                    onPageSizeChange={(size) => patchState({ page: 1, pageSize: size })}
                    pageSizeOptions={TABLE_PAGE_SIZE_OPTIONS}
                />
            </TableFooter>
        </Box>
    );
}
