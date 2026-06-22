"use client";

import { Card, Flex, Group, Skeleton, Stack, Text } from "@mantine/core";
import { useMounted } from "@mantine/hooks";
import { DonutChart } from "@mantine/charts";
import type { MetodoPagoDto } from "@lib/types/dashboard.types";
import { formatMoneda } from "@lib/utils/money.utils";

const COLORS = [
    "oklch(62% 0.15 245)",
    "oklch(58% 0.09 185)",
    "oklch(55% 0.12 288)",
    "oklch(63% 0.13 63)",
    "oklch(60% 0.13 150)",
    "oklch(58% 0.13 345)",
];

export function MetodosPagoChart({ data }: { data: MetodoPagoDto[] }) {
    const mounted = useMounted();
    const chartData = data.flatMap((m, i) => {
        const value = Number(m.total) || 0;
        if (value <= 0) return [];
        return [{ name: m.detalle || m.codigo, value, color: COLORS[i % COLORS.length] }];
    });

    return (
        <Card padding="xl" className="h-full shadow-xs">
            <Stack gap="lg">
                <Stack gap={2}>
                    <Text
                        component="span"
                        className="text-[11px] font-semibold uppercase text-theme-text-dim"
                    >
                        Cobro
                    </Text>
                    <Text className="font-semibold text-theme-text">
                        Ventas por método de pago
                    </Text>
                </Stack>
                {chartData.length === 0 ? (
                    <Text size="sm" className="text-theme-text-muted">
                        Sin pagos en el periodo.
                    </Text>
                ) : !mounted ? (
                    <Skeleton h={180} radius="md" />
                ) : (
                    <Flex
                        direction={{ base: "column", sm: "row" }}
                        justify="center"
                        align="center"
                        gap="xl"
                        py="sm"
                    >
                        <DonutChart
                            w={180}
                            h={180}
                            data={chartData}
                            withTooltip
                            tooltipDataSource="segment"
                            thickness={28}
                            valueFormatter={(value) => formatMoneda(value)}
                            style={{ width: 180, height: 180, flexShrink: 0 }}
                        />
                        <Stack gap={6} className="w-full sm:w-auto sm:min-w-40">
                            {chartData.map((d) => (
                                <Group key={d.name} gap={8} wrap="nowrap">
                                    <span
                                        className="inline-block size-3 rounded-sm"
                                        style={{ backgroundColor: d.color }}
                                    />
                                    <Text size="xs" className="text-theme-text">
                                        {d.name}
                                    </Text>
                                    <Text
                                        size="xs"
                                        className="text-theme-text-dim tabular-nums ml-auto"
                                    >
                                        {formatMoneda(d.value)}
                                    </Text>
                                </Group>
                            ))}
                        </Stack>
                    </Flex>
                )}
            </Stack>
        </Card>
    );
}
