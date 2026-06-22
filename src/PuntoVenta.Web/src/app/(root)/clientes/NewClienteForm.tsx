"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearClienteAction } from "@lib/actions/clientes.actions";
import { CLIENTE_FIELDS } from "@lib/constants/clientes.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearClienteSchema } from "@lib/schemas/clientes.schema";
import type { CrearClienteFormValues } from "@lib/types/clientes.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { Alert, Button, Grid } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { ClienteFormFields } from "@pages/clientes/ClienteFormFields";

export function NewClienteForm() {
    const queryClient = useQueryClient();
    const form = useForm<CrearClienteFormValues>({
        initialValues: {
            [CLIENTE_FIELDS.NOMBRE]: "",
            [CLIENTE_FIELDS.IDENTIFICACION]: "",
            [CLIENTE_FIELDS.CORREO]: "",
            [CLIENTE_FIELDS.TELEFONO]: "",
            [CLIENTE_FIELDS.OBSERVACIONES]: "",
        },
        validate: zodResolver(crearClienteSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearClienteFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.clientes.all,
                });
                AppNotifier.success({
                    message: "Cliente guardado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CrearClienteFormValues) {
        await execute(() => crearClienteAction(values));
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Grid gap="md">
                {error && (
                    <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                        {error}
                    </Alert>
                )}
                <Grid.Col span={12}>
                    <ClienteFormFields form={form} />
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear cliente
                </Button>
            </Grid>
        </form>
    );
}
