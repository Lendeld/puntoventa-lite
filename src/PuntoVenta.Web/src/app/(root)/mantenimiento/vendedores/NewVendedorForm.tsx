"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearVendedorAction } from "@lib/actions/vendedores.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { VENDEDOR_FIELDS } from "@lib/constants/vendedores.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearVendedorSchema } from "@lib/schemas/vendedores.schema";
import { obtenerVendedoresActivosService } from "@lib/services/vendedores.service";
import type { CrearVendedorFormValues } from "@lib/types/vendedores.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { Alert, Button, Checkbox, Grid, Text, TextInput } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";

const NOMBRE_MAX = 150;

export function NewVendedorForm() {
    const queryClient = useQueryClient();
    const { data: vendedoresActivos } = useQuery({
        queryKey: QUERY_KEYS.vendedores.activas,
        queryFn: async () => {
            const res = await obtenerVendedoresActivosService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });
    const principalExiste = (vendedoresActivos ?? []).some((item) => item.isPrincipal);

    const form = useForm<CrearVendedorFormValues>({
        initialValues: {
            [VENDEDOR_FIELDS.NOMBRE]: "",
            [VENDEDOR_FIELDS.IS_PRINCIPAL]: !principalExiste,
        },
        validate: zodResolver(crearVendedorSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearVendedorFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.vendedores.all,
                });
                AppNotifier.success({
                    message: "Vendedor guardado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CrearVendedorFormValues) {
        await execute(() => crearVendedorAction(values));
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
                        placeholder="Nombre del vendedor"
                        required
                        maxLength={NOMBRE_MAX}
                        key={form.key(VENDEDOR_FIELDS.NOMBRE)}
                        {...form.getInputProps(VENDEDOR_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Checkbox
                        label="Principal"
                        description={
                            principalExiste
                                ? "Si lo marcas como principal, reemplazará al principal actual."
                                : "El primer vendedor quedará como principal."
                        }
                        key={form.key(VENDEDOR_FIELDS.IS_PRINCIPAL)}
                        disabled={!principalExiste}
                        {...form.getInputProps(VENDEDOR_FIELDS.IS_PRINCIPAL, {
                            type: "checkbox",
                        })}
                    />
                    {!principalExiste && (
                        <Text size="xs" c="dimmed" mt={4}>
                            Aún no existe un vendedor principal.
                        </Text>
                    )}
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear vendedor
                </Button>
            </Grid>
        </form>
    );
}
