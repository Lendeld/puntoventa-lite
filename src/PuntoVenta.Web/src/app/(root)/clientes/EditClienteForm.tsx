"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarClienteAction } from "@lib/actions/clientes.actions";
import { CLIENTE_FIELDS } from "@lib/constants/clientes.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarClienteSchema } from "@lib/schemas/clientes.schema";
import { obtenerClientePorIdService } from "@lib/services/clientes.service";
import type { ActualizarClienteFormValues } from "@lib/types/clientes.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    Checkbox,
    Divider,
    Grid,
    Loader,
    Stack,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { ClienteFormFields } from "@pages/clientes/ClienteFormFields";

interface Props {
    id: string;
}

export default function EditClienteForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedClienteIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarClienteFormValues>({
        initialValues: {
            [CLIENTE_FIELDS.NOMBRE]: "",
            [CLIENTE_FIELDS.IDENTIFICACION]: "",
            [CLIENTE_FIELDS.CORREO]: "",
            [CLIENTE_FIELDS.TELEFONO]: "",
            [CLIENTE_FIELDS.OBSERVACIONES]: "",
            [CLIENTE_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarClienteSchema),
    });

    const {
        data: cliente,
        isLoading: loadingCliente,
        isError: isClienteError,
    } = useQuery({
        queryKey: QUERY_KEYS.clientes.detalle(id),
        queryFn: async () => {
            const res = await obtenerClientePorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnMount: "always",
    });

    useEffect(() => {
        if (!cliente) return;
        if (hydratedClienteIdRef.current === cliente.id) return;

        form.setValues({
            [CLIENTE_FIELDS.NOMBRE]: cliente.nombre,
            [CLIENTE_FIELDS.IDENTIFICACION]: cliente.identificacion ?? "",
            [CLIENTE_FIELDS.CORREO]: cliente.correo ?? "",
            [CLIENTE_FIELDS.TELEFONO]: cliente.telefono ?? "",
            [CLIENTE_FIELDS.OBSERVACIONES]: cliente.observaciones ?? "",
            [CLIENTE_FIELDS.ACTIVO]: cliente.activo,
        });
        form.resetDirty();
        hydratedClienteIdRef.current = cliente.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [cliente]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [cliente]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarClienteFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.clientes.all,
                });
                AppNotifier.success({
                    message: "Cliente actualizado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarClienteFormValues) {
        await execute(() => actualizarClienteAction(id, values));
    }

    if (loadingCliente) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isClienteError || !cliente) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                Error al cargar datos del cliente.
            </Alert>
        );
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
                <Divider className="w-full" />
                <Grid.Col span={12}>
                    <Checkbox
                        label="Activo"
                        key={form.key(CLIENTE_FIELDS.ACTIVO)}
                        {...form.getInputProps(CLIENTE_FIELDS.ACTIVO, {
                            type: "checkbox",
                        })}
                    />
                </Grid.Col>
                <Button
                    type="submit"
                    loading={loading}
                    fullWidth
                    mt="xs"
                    disabled={!form.isDirty()}
                >
                    Guardar cambios
                </Button>
            </Grid>
        </form>
    );
}
