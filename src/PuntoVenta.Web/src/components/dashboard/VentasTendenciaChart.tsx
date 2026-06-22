"use client";

import { Card, Group, Skeleton, Stack, Text } from "@mantine/core";
import { useMounted } from "@mantine/hooks";
import { AreaChart } from "@mantine/charts";
import dayjs from "dayjs";
import type { PuntoTendenciaDto } from "@lib/types/dashboard.types";
import { formatMoneda } from "@lib/utils/money.utils";

function formatEje(value: number): string {
    if (value >= 1_000_000) return `₡${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `₡${(value / 1_000).toFixed(0)}K`;
    return `₡${value}`;
}

export function VentasTendenciaChart({ data }: { data: PuntoTendenciaDto[] }) {
    const mounted = useMounted();
    const chartData = data.map((p) => ({
        fecha: dayjs(p.fecha).format("DD/MM"),
        total: p.total,
    }));

    return (
        <Card padding="xl" className="h-full shadow-xs">
            <Stack gap="lg">
                <Group justify="space-between" align="flex-start" gap="md">
                    <Stack gap={2}>
                        <Text
                            component="span"
                            className="text-[11px] font-semibold uppercase text-theme-text-dim"
                        >
                            Tendencia
                        </Text>
                        <Text className="font-semibold text-theme-text">
                            Ventas últimos 30 días
                        </Text>
                    </Stack>
                    <Text size="xs" className="text-theme-text-muted">
                        CRC
                    </Text>
                </Group>
                {chartData.length === 0 ? (
                    <Text size="sm" className="text-theme-text-muted">
                        Sin ventas en el periodo.
                    </Text>
                ) : !mounted ? (
                    <Skeleton h={240} radius="md" />
                ) : (
                    <AreaChart
                        h={240}
                        data={chartData}
                        dataKey="fecha"
                        series={[
                            { name: "total", label: "Ventas", color: "accentPV.6" },
                        ]}
                        curveType="natural"
                        withDots={false}
                        gridAxis="y"
                        yAxisProps={{
                            width: 72,
                            tickFormatter: formatEje,
                        }}
                        valueFormatter={(value) => formatMoneda(value)}
                    />
                )}
            </Stack>
        </Card>
    );
}
