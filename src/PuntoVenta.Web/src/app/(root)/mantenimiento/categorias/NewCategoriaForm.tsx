"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { crearCategoriaAction } from "@lib/actions/categorias.actions";
import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearCategoriaSchema } from "@lib/schemas/categorias.schema";
import type { CrearCategoriaFormValues } from "@lib/types/categorias.types";
import { zodResolver } from "@lib/utils/zodResolver";
import { Alert, Button, Grid, TextInput, Textarea } from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";

const NOMBRE_MAX = 150;
const DESCRIPCION_MAX = 255;

export function NewCategoriaForm() {
    const queryClient = useQueryClient();
    const form = useForm<CrearCategoriaFormValues>({
        initialValues: {
            [CATEGORIA_FIELDS.NOMBRE]: "",
            [CATEGORIA_FIELDS.DESCRIPCION]: "",
        },
        validate: zodResolver(crearCategoriaSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearCategoriaFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.categorias.all,
                });
                AppNotifier.success({
                    message: "Categoría guardada exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CrearCategoriaFormValues) {
        await execute(() => crearCategoriaAction(values));
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
                        placeholder="Nombre de la categoría"
                        required
                        maxLength={NOMBRE_MAX}
                        key={form.key(CATEGORIA_FIELDS.NOMBRE)}
                        {...form.getInputProps(CATEGORIA_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Textarea
                        label="Descripción"
                        placeholder="Descripción de la categoría"
                        autosize
                        minRows={4}
                        maxLength={DESCRIPCION_MAX}
                        key={form.key(CATEGORIA_FIELDS.DESCRIPCION)}
                        {...form.getInputProps(CATEGORIA_FIELDS.DESCRIPCION)}
                    />
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear categoría
                </Button>
            </Grid>
        </form>
    );
}
