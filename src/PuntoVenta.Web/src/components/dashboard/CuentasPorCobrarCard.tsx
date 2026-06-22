"use client";

import { Card, Group, Text, ThemeIcon } from "@mantine/core";
import { IconAlertTriangle } from "@tabler/icons-react";
import type { CuentasPorCobrarDto } from "@lib/types/dashboard.types";
import { formatMoneda } from "@lib/utils/money.utils";

interface Props {
    data: CuentasPorCobrarDto;
    className?: string;
}

export function CuentasPorCobrarCard({ data, className = "" }: Props) {
    const hayVencidas = data.cantidadVencidas > 0;

    return (
        <Card
            padding="xl"
            className={`h-full shadow-xs ${
                hayVencidas
                    ? "border-theme-danger/35 bg-theme-danger-soft"
                    : ""
            } ${className}`.trim()}
        >
            <Group justify="space-between" align="center" wrap="nowrap">
                <Text
                    component="span"
                    className="min-w-0 text-[11px] font-semibold uppercase text-theme-text-dim"
                >
                    Cobros vencidos
                </Text>
                <ThemeIcon
                    variant="light"
                    color={hayVencidas ? "red" : "gray"}
                    size="lg"
                    radius="md"
                    className="shrink-0"
                >
                    <IconAlertTriangle size={18} />
                </ThemeIcon>
            </Group>
            <Text className="mt-3 font-display text-3xl leading-tight text-theme-text tabular-nums [overflow-wrap:anywhere]">
                {formatMoneda(data.totalVencido)}
            </Text>
            <Text size="xs" className="text-theme-text-muted mt-2">
                {hayVencidas
                    ? `${data.cantidadVencidas} facturas vencidas`
                    : "Sin facturas vencidas"}
            </Text>
        </Card>
    );
}
