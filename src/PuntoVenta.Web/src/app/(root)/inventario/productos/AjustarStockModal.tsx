"use client";

import { AJUSTE_STOCK_FIELDS } from "@lib/constants/inventario.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ajustarStockAction } from "@lib/actions/inventario.actions";
import { ajusteStockSchema } from "@lib/schemas/inventario.schema";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { zodResolver } from "@lib/utils/zodResolver";
import type { AjusteStockFormValues } from "@lib/schemas/inventario.schema";
import { Button, Group, NumberInput, Stack, Text, Textarea } from "@mantine/core";
import { useForm } from "@mantine/form";
import { useQueryClient } from "@tanstack/react-query";

interface Props {
    productoId: string;
    productoNombre: string;
    onClose: () => void;
}

export default function AjustarStockModal({
    productoId,
    productoNombre,
    onClose,
}: Props) {
    const queryClient = useQueryClient();

    const form = useForm<AjusteStockFormValues>({
        initialValues: {
            [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: productoId,
            [AJUSTE_STOCK_FIELDS.DELTA]: 0,
            [AJUSTE_STOCK_FIELDS.RAZON]: "",
        },
        validate: zodResolver(ajusteStockSchema),
    });

    const { execute, loading } = useActionHandler({
        form,
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: QUERY_KEYS.productos.all });
            void queryClient.invalidateQueries({ queryKey: QUERY_KEYS.inventario.all });
            onClose();
        },
    });

    return (
        <form
            onSubmit={form.onSubmit((values) =>
                execute(() => ajustarStockAction(values)),
            )}
            noValidate
        >
            <Stack gap="md">
                <Text size="sm" c="dimmed">
                    Producto: <strong>{productoNombre}</strong>
                </Text>
                <NumberInput
                    key={form.key(AJUSTE_STOCK_FIELDS.DELTA)}
                    label="Cantidad"
                    description="Positivo para ingreso, negativo para salida."
                    placeholder="Ej: 10 o -5"
                    allowNegative
                    allowDecimal
                    decimalScale={5}
                    {...form.getInputProps(AJUSTE_STOCK_FIELDS.DELTA)}
                />
                <Textarea
                    key={form.key(AJUSTE_STOCK_FIELDS.RAZON)}
                    label="Razón / Observación"
                    placeholder="Descripción del ajuste (opcional)"
                    maxLength={255}
                    rows={3}
                    {...form.getInputProps(AJUSTE_STOCK_FIELDS.RAZON)}
                />
                <Group justify="flex-end" gap="sm">
                    <Button variant="light" color="gray" onClick={onClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button type="submit" loading={loading}>
                        Aplicar ajuste
                    </Button>
                </Group>
            </Stack>
        </form>
    );
}
