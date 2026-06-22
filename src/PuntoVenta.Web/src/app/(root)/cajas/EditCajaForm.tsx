"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarCajaAction } from "@lib/actions/cajas.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { CAJA_FIELDS, CAJA_MAX } from "@lib/constants/cajas.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarCajaSchema } from "@lib/schemas/cajas.schema";
import type { ActualizarCajaFormValues, CajaListadoItemDto } from "@lib/types/cajas.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    Grid,
    Stack,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";

interface Props {
    caja: CajaListadoItemDto;
}

export default function EditCajaForm({ caja }: Props) {
    const queryClient = useQueryClient();
    const hydratedIdRef = useRef<string | null>(null);

    const form = useForm<ActualizarCajaFormValues>({
        initialValues: {
            [CAJA_FIELDS.CODIGO]: "",
            [CAJA_FIELDS.NOMBRE]: "",
        },
        validate: zodResolver(actualizarCajaSchema),
    });

    useEffect(() => {
        if (hydratedIdRef.current === caja.id) return;

        form.setValues({
            [CAJA_FIELDS.CODIGO]: caja.codigo,
            [CAJA_FIELDS.NOMBRE]: caja.nombre,
        });
        form.resetDirty();
        hydratedIdRef.current = caja.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [caja]
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [caja]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarCajaFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.cajas.all,
                });
                AppNotifier.success({ message: "Caja actualizada exitosamente." });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarCajaFormValues) {
        await execute(() => actualizarCajaAction(caja.id, values));
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Stack gap="md">
                {error && (
                    <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                        {error}
                    </Alert>
                )}
                <Grid>
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
                </Grid>
                <Button
                    type="submit"
                    loading={loading}
                    fullWidth
                    mt="xs"
                    disabled={!form.isDirty()}
                >
                    Guardar cambios
                </Button>
            </Stack>
        </form>
    );
}
