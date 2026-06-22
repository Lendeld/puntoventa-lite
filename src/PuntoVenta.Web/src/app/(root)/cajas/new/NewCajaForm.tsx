"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearCajaAction } from "@lib/actions/cajas.actions";
import { CAJA_FIELDS, CAJA_MAX } from "@lib/constants/cajas.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearCajaSchema } from "@lib/schemas/cajas.schema";
import type { CrearCajaFormValues } from "@lib/types/cajas.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    Grid,
    Group,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";

export function NewCajaForm() {
    const queryClient = useQueryClient();
    const form = useForm<CrearCajaFormValues>({
        initialValues: {
            [CAJA_FIELDS.CODIGO]: "",
            [CAJA_FIELDS.NOMBRE]: "",
        },
        validate: zodResolver(crearCajaSchema),
    });

    const { execute, loading, error, setError } = useActionHandler<CrearCajaFormValues>({
        form,
        onSuccess: async () => {
            await queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.cajas.all,
            });
            AppNotifier.success({ message: "Caja creada exitosamente." });
            modals.closeAll();
        },
    });

    async function handleSubmit(values: CrearCajaFormValues) {
        await execute(() => crearCajaAction(values));
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Grid gap="md">
                {error && (
                    <Grid.Col span={12}>
                        <Alert
                            icon={<IconAlertCircle size={16} />}
                            variant="light"
                            color="red"
                        >
                            {error}
                        </Alert>
                    </Grid.Col>
                )}
                <Grid.Col span={{ base: 12, sm: 4 }}>
                    <TextInput
                        label="Código"
                        placeholder="01"
                        required
                        maxLength={CAJA_MAX.CODIGO}
                        key={form.key(CAJA_FIELDS.CODIGO)}
                        {...form.getInputProps(CAJA_FIELDS.CODIGO)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 8 }}>
                    <TextInput
                        label="Nombre"
                        placeholder="Caja sucursal centro"
                        required
                        maxLength={CAJA_MAX.NOMBRE}
                        key={form.key(CAJA_FIELDS.NOMBRE)}
                        {...form.getInputProps(CAJA_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Group justify="flex-end" gap="sm">
                        <Button
                            variant="light"
                            onClick={() => modals.closeAll()}
                            disabled={loading}
                        >
                            Cancelar
                        </Button>
                        <Button type="submit" loading={loading}>
                            Crear caja
                        </Button>
                    </Group>
                </Grid.Col>
            </Grid>
        </form>
    );
}
