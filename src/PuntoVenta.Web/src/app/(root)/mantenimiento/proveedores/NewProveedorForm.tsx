"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearProveedorAction } from "@lib/actions/proveedores.actions";
import { PROVEEDOR_FIELDS, PROVEEDOR_MAX } from "@lib/constants/proveedores.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearProveedorSchema } from "@lib/schemas/proveedores.schema";
import type { CrearProveedorFormValues } from "@lib/types/proveedores.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { Alert, Button, Grid, TextInput, Textarea } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";

export function NewProveedorForm() {
    const queryClient = useQueryClient();
    const form = useForm<CrearProveedorFormValues>({
        initialValues: {
            [PROVEEDOR_FIELDS.NOMBRE]: "",
            [PROVEEDOR_FIELDS.CORREO]: "",
            [PROVEEDOR_FIELDS.TELEFONO]: "",
            [PROVEEDOR_FIELDS.OBSERVACION]: "",
        },
        validate: zodResolver(crearProveedorSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearProveedorFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.proveedores.all,
                });
                AppNotifier.success({
                    message: "Proveedor guardado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CrearProveedorFormValues) {
        await execute(() => crearProveedorAction(values));
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
                    <TextInput
                        label="Nombre"
                        placeholder="Nombre del proveedor"
                        required
                        maxLength={PROVEEDOR_MAX.NOMBRE}
                        key={form.key(PROVEEDOR_FIELDS.NOMBRE)}
                        {...form.getInputProps(PROVEEDOR_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <TextInput
                        label="Correo"
                        placeholder="correo@ejemplo.com"
                        type="email"
                        maxLength={PROVEEDOR_MAX.CORREO}
                        key={form.key(PROVEEDOR_FIELDS.CORREO)}
                        {...form.getInputProps(PROVEEDOR_FIELDS.CORREO)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <TextInput
                        label="Teléfono"
                        placeholder="2222-3333"
                        maxLength={PROVEEDOR_MAX.TELEFONO}
                        key={form.key(PROVEEDOR_FIELDS.TELEFONO)}
                        {...form.getInputProps(PROVEEDOR_FIELDS.TELEFONO)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Textarea
                        label="Observación"
                        placeholder="Notas adicionales"
                        autosize
                        minRows={3}
                        maxLength={PROVEEDOR_MAX.OBSERVACION}
                        key={form.key(PROVEEDOR_FIELDS.OBSERVACION)}
                        {...form.getInputProps(PROVEEDOR_FIELDS.OBSERVACION)}
                    />
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear proveedor
                </Button>
            </Grid>
        </form>
    );
}
