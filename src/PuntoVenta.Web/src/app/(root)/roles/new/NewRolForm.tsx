"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearRolAction } from "@lib/actions/roles.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ROL_FIELDS } from "@lib/constants/roles.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearRolSchema } from "@lib/schemas/roles.schema";
import type { CrearRolFormValues } from "@lib/types/roles.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { Alert, Button, Grid, TextInput, Textarea } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";

const NOMBRE_MAX = 100;
const DESCRIPCION_MAX = 500;

export function NewRolForm() {
    const queryClient = useQueryClient();
    const form = useForm<CrearRolFormValues>({
        initialValues: {
            [ROL_FIELDS.NOMBRE]: "",
            [ROL_FIELDS.DESCRIPCION]: "",
        },
        validate: zodResolver(crearRolSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearRolFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.roles.all,
                });
                AppNotifier.success({
                    message: "Rol guardado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CrearRolFormValues) {
        await execute(() => crearRolAction(values));
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Grid gap="md">
                {error && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        variant="light"
                        color="red"
                    >
                        {error}
                    </Alert>
                )}
                <Grid.Col span={12}>
                    <TextInput
                        label="Nombre"
                        placeholder="Nombre del rol"
                        required
                        maxLength={NOMBRE_MAX}
                        key={form.key(ROL_FIELDS.NOMBRE)}
                        {...form.getInputProps(ROL_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Textarea
                        label="Descripción"
                        placeholder="Descripción del rol"
                        autosize
                        minRows={4}
                        maxLength={DESCRIPCION_MAX}
                        key={form.key(ROL_FIELDS.DESCRIPCION)}
                        {...form.getInputProps(ROL_FIELDS.DESCRIPCION)}
                    />
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear rol
                </Button>
            </Grid>
        </form>
    );
}
