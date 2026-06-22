"use client";

import { Badge, Group, Stack, Text, ThemeIcon } from "@mantine/core";
import {
    IconArrowRight,
    IconFileSpreadsheet,
} from "@tabler/icons-react";
import type { Icon } from "@tabler/icons-react";
import Link from "next/link";
import { usePermisoQuery } from "@lib/hooks/usePermisoQuery";

export interface ReporteEntrada {
    titulo: string;
    descripcion: string;
    href: string;
    capacidades: string[];
    icono: Icon;
    permiso?: string;
}

export function ReporteCard({ reporte }: { reporte: ReporteEntrada }) {
    const { data: allowed, isLoading } = usePermisoQuery(reporte.permiso ?? "");

    if (reporte.permiso && (isLoading || !allowed)) return null;

    const Icono = reporte.icono;

    return (
        <Link
            href={reporte.href}
            className="group block rounded-lg border border-theme bg-theme-surface p-5 transition-colors duration-(--dur-fast) ease-soft hover:border-theme-accent-border hover:bg-theme-surface-2 focus:outline-none focus-visible:shadow-(--ring-accent)"
        >
            <Stack gap="md" className="h-full">
                <Group justify="space-between" align="flex-start" wrap="nowrap">
                    <ThemeIcon size={38} radius="md" variant="light" color="accentPV">
                        <Icono size={20} />
                    </ThemeIcon>
                    <IconArrowRight
                        size={18}
                        className="mt-1 shrink-0 text-theme-text-dim transition-transform duration-(--dur-fast) ease-soft group-hover:translate-x-0.5 group-hover:text-theme"
                    />
                </Group>

                <Stack gap={4} className="flex-1">
                    <Text fw={600} size="md">
                        {reporte.titulo}
                    </Text>
                    <Text size="sm" c="dimmed" className="leading-relaxed">
                        {reporte.descripcion}
                    </Text>
                </Stack>

                <Group gap={6} wrap="wrap">
                    {reporte.capacidades.map((cap) => (
                        <Badge
                            key={cap}
                            size="sm"
                            variant="default"
                            leftSection={
                                cap === "Exporta a Excel" ? (
                                    <IconFileSpreadsheet size={11} />
                                ) : undefined
                            }
                        >
                            {cap}
                        </Badge>
                    ))}
                </Group>
            </Stack>
        </Link>
    );
}
