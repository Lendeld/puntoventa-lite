"use client";

import { ColumnDefinition } from "@lib/types/base.types";
import { Table, Box, LoadingOverlay } from "@mantine/core";
import { IconAlertCircle } from "@tabler/icons-react";

interface Props<T> {
    columns: ColumnDefinition<T>[];
    data: T[];
    loading?: boolean;
    emptyText?: string;
    error?: string;
    getRowId: (row: T) => string;
    getRowClassName?: (row: T) => string | undefined;
}

function alignClass(align?: "left" | "center" | "right"): string {
    if (align === "right") return "text-right";
    if (align === "center") return "text-center";
    return "text-left";
}

export function DataTable<T>({
    columns,
    data,
    loading = false,
    emptyText = "No hay registros",
    error,
    getRowId,
    getRowClassName,
}: Props<T>) {
    return (
        <Box className="relative">
            <LoadingOverlay
                visible={loading}
                zIndex={1000}
                overlayProps={{ radius: "sm", blur: 2 }}
                loaderProps={{ color: "accentPV", type: "bars" }}
                className="h-table"
            />
            <Table
                highlightOnHover
                withTableBorder={false}
                verticalSpacing="sm"
            >
                <Table.Thead>
                    <Table.Tr>
                        {columns.map((col) => (
                            <Table.Th
                                key={col.key}
                                className={alignClass(col.align)}
                                style={
                                    col.width ? { width: col.width } : undefined
                                }
                            >
                                {col.header}
                            </Table.Th>
                        ))}
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {error ? (
                        <Table.Tr>
                            <Table.Td
                                colSpan={columns.length}
                                className="text-center py-10 h-table-body"
                            >
                                <div className="flex flex-col items-center gap-2 text-theme-danger">
                                    <IconAlertCircle size={28} />
                                    <span className="text-sm">{error}</span>
                                </div>
                            </Table.Td>
                        </Table.Tr>
                    ) : data.length === 0 ? (
                        <Table.Tr>
                            <Table.Td
                                colSpan={columns.length}
                                className="text-center text-theme-text-muted py-10 h-table-body"
                            >
                                {emptyText}
                            </Table.Td>
                        </Table.Tr>
                    ) : (
                        data.map((row) => (
                            <Table.Tr
                                key={getRowId(row)}
                                className={getRowClassName?.(row)}
                            >
                                {columns.map((col) => (
                                    <Table.Td
                                        key={col.key}
                                        className={alignClass(col.align)}
                                    >
                                        {col.cell(row)}
                                    </Table.Td>
                                ))}
                            </Table.Tr>
                        ))
                    )}
                </Table.Tbody>
            </Table>
        </Box>
    );
}
