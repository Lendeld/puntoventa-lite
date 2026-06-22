"use client";

import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { Card, Stack, Text, Textarea } from "@mantine/core";
import type { ReactNode } from "react";

interface Props {
    values: Record<string, unknown>;
    errors: Record<string, ReactNode>;
    disabled: boolean;
    onFieldChange: (field: string, value: unknown) => void;
}

export function FacturacionObservacionesCard({
    values,
    errors,
    disabled,
    onFieldChange,
}: Props) {
    return (
        <Card
            radius="lg"
            p="lg"
            className="h-full"
        >
            <Stack gap="md">
                <Stack gap={2}>
                    <Text fw={700}>Observaciones</Text>
                    <Text size="sm" c="dimmed">
                        Notas opcionales para esta factura.
                    </Text>
                </Stack>

                <Textarea
                    placeholder="Escribe una observacion si hace falta"
                    autosize
                    minRows={3}
                    value={String(values[VENTA_FIELDS.OBSERVACIONES] ?? "")}
                    onChange={(event) => onFieldChange(VENTA_FIELDS.OBSERVACIONES, event.currentTarget.value)}
                    error={errors[VENTA_FIELDS.OBSERVACIONES]}
                    disabled={disabled}
                />
            </Stack>
        </Card>
    );
}
