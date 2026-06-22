"use client";

import { Group, Stack, Text, ThemeIcon } from "@mantine/core";
import type { Icon } from "@tabler/icons-react";
import { ReporteCard, type ReporteEntrada } from "@ui/reportes/ReporteCard";

interface ReportesHubProps {
    titulo: string;
    descripcion: string;
    icono: Icon;
    reportes: ReporteEntrada[];
}

export function ReportesHub({
    titulo,
    descripcion,
    icono: Icono,
    reportes,
}: ReportesHubProps) {
    return (
        <Stack gap="xl" className="mx-auto w-full max-w-4xl">
            <Group gap="md" wrap="nowrap" align="flex-start">
                <ThemeIcon size={48} radius="md" variant="light" color="accentPV">
                    <Icono size={26} />
                </ThemeIcon>
                <Stack gap={2}>
                    <Text fw={700} size="xl">
                        {titulo}
                    </Text>
                    <Text size="sm" c="dimmed" className="max-w-prose">
                        {descripcion}
                    </Text>
                </Stack>
            </Group>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                {reportes.map((reporte) => (
                    <ReporteCard key={reporte.href} reporte={reporte} />
                ))}
            </div>
        </Stack>
    );
}
