"use client";

import { Card, Group, Text, ThemeIcon } from "@mantine/core";
import { IconArrowDownRight, IconArrowUpRight } from "@tabler/icons-react";
import type { ReactNode } from "react";

type Tone = "default" | "alert" | "warn";
type Variant = "default" | "featured";

interface Props {
    label: string;
    value: string;
    icon?: ReactNode;
    delta?: number | null;
    subtitle?: string;
    tone?: Tone;
    variant?: Variant;
    className?: string;
}

const TONE_CARD: Record<Tone, string> = {
    default: "",
    alert: "border-theme-danger/35 bg-theme-danger-soft",
    warn: "border-theme-warning/35 bg-theme-warning-soft",
};

const VARIANT_CARD: Record<Variant, string> = {
    default: "",
    featured:
        "border-theme-accent-border bg-[oklch(95.5%_0.018_255)] dark:bg-[oklch(21%_0.018_255)]",
};

export function KpiCard({
    label,
    value,
    icon,
    delta,
    subtitle,
    tone = "default",
    variant = "default",
    className = "",
}: Props) {
    const hasDelta = delta !== null && delta !== undefined;
    const positivo = (delta ?? 0) >= 0;
    const valueClass =
        variant === "featured"
            ? "font-display text-4xl text-theme-text tabular-nums"
            : "font-display text-3xl text-theme-text tabular-nums";

    return (
        <Card
            padding="xl"
            className={`h-full shadow-xs ${VARIANT_CARD[variant]} ${TONE_CARD[tone]} ${className}`.trim()}
        >
            <Group justify="space-between" align="center" wrap="nowrap">
                <Text
                    component="span"
                    className="min-w-0 text-[11px] font-semibold uppercase text-theme-text-dim"
                >
                    {label}
                </Text>
                {icon && (
                    <ThemeIcon
                        variant="light"
                        size={variant === "featured" ? 42 : "lg"}
                        radius="md"
                        className="shrink-0"
                    >
                        {icon}
                    </ThemeIcon>
                )}
            </Group>
            <Text className={`${valueClass} mt-3 leading-tight [overflow-wrap:anywhere]`}>
                {value}
            </Text>
            <Group gap={8} mt="lg" wrap="nowrap" className="min-h-5">
                {hasDelta && (
                    <Group
                        gap={2}
                        wrap="nowrap"
                        className={
                            positivo
                                ? "text-green-600 dark:text-green-400"
                                : "text-red-600 dark:text-red-400"
                        }
                    >
                        {positivo ? (
                            <IconArrowUpRight size={15} />
                        ) : (
                            <IconArrowDownRight size={15} />
                        )}
                        <Text size="xs" className="font-semibold tabular-nums">
                            {Math.abs(delta as number).toFixed(1)}%
                        </Text>
                    </Group>
                )}
                {subtitle && (
                    <Text size="xs" className="text-theme-text-muted truncate">
                        {subtitle}
                    </Text>
                )}
            </Group>
        </Card>
    );
}
