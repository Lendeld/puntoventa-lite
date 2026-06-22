"use client";

import { Pagination, Select, Text } from "@mantine/core";
import { TABLE_PAGE_SIZE_OPTIONS } from "@lib/constants/table.constants";

interface Props {
    page: number;
    pageSize: number;
    total: number;
    pageSizeOptions?: readonly number[];
    totalPages: number;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
}

export function TablePagination({
    page,
    pageSize,
    total,
    pageSizeOptions = TABLE_PAGE_SIZE_OPTIONS,
    totalPages,
    onPageChange,
    onPageSizeChange,
}: Props) {
    const from = total === 0 ? 0 : (page - 1) * pageSize + 1;
    const to = Math.min(page * pageSize, total);

    return (
        <div className="flex flex-wrap items-center justify-between gap-3">
            <Text size="sm" className="text-theme-text-muted">
                Mostrando {from}–{to} de {total} registros
            </Text>
            <Pagination
                total={totalPages}
                value={page}
                onChange={onPageChange}
                siblings={1}
                boundaries={1}
                size="sm"
            />
            <Select
                value={String(pageSize)}
                onChange={(v) => v && onPageSizeChange(Number(v))}
                data={pageSizeOptions.map((n) => ({
                    value: String(n),
                    label: String(n),
                }))}
                comboboxProps={{ withinPortal: true }}
                w={80}
                size="sm"
                allowDeselect={false}
                classNames={{
                    input: "bg-theme-surface text-theme-text",
                }}
            />
        </div>
    );
}
