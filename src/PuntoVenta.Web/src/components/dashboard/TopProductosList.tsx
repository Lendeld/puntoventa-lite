"use client";

import { Box, Card, Group, Stack, Text } from "@mantine/core";
import type { TopProductoDto } from "@lib/types/dashboard.types";
import { formatMoneda, formatNumero } from "@lib/utils/money.utils";

export function TopProductosList({ data }: { data: TopProductoDto[] }) {
    return (
        <Card padding="xl" className="h-full shadow-xs">
            <Stack gap="md">
                <Group justify="space-between" align="flex-start" gap="md">
                    <Stack gap={2}>
                        <Text
                            component="span"
                            className="text-[11px] font-semibold uppercase text-theme-text-dim"
                        >
                            Productos
                        </Text>
                        <Text className="font-semibold text-theme-text">
                            Más vendidos del mes
                        </Text>
                    </Stack>
                    <Text size="xs" className="text-theme-text-muted tabular-nums">
                        {data.length} líneas
                    </Text>
                </Group>
                {data.length === 0 ? (
                    <Text size="sm" className="text-theme-text-muted">
                        Aún no hay ventas este mes.
                    </Text>
                ) : (
                    data.map((p, i) => (
                        <Box
                            key={p.nombre}
                            className="rounded-md bg-theme-surface-2/70 px-3 py-2.5"
                        >
                            <Group
                                justify="space-between"
                                wrap="nowrap"
                                align="center"
                            >
                                <Group gap="sm" wrap="nowrap" className="min-w-0">
                                    <Text className="grid size-6 shrink-0 place-items-center rounded-sm bg-theme-accent-soft text-xs font-semibold text-theme tabular-nums">
                                        {i + 1}
                                    </Text>
                                    <Text
                                        size="sm"
                                        className="truncate font-medium text-theme-text"
                                    >
                                        {p.nombre}
                                    </Text>
                                </Group>
                                <Stack gap={0} align="flex-end" className="shrink-0">
                                    <Text
                                        size="sm"
                                        className="text-theme-text tabular-nums"
                                    >
                                        {formatMoneda(p.total)}
                                    </Text>
                                    <Text
                                        size="xs"
                                        className="text-theme-text-dim tabular-nums"
                                    >
                                        {formatNumero(p.cantidad)} uds
                                    </Text>
                                </Stack>
                            </Group>
                        </Box>
                    ))
                )}
            </Stack>
        </Card>
    );
}
