"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarVendedorAction } from "@lib/actions/vendedores.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { VENDEDOR_FIELDS } from "@lib/constants/vendedores.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarVendedorSchema } from "@lib/schemas/vendedores.schema";
import { obtenerVendedorPorIdService } from "@lib/services/vendedores.service";
import type { ActualizarVendedorFormValues } from "@lib/types/vendedores.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    Checkbox,
    Divider,
    Grid,
    Loader,
    Stack,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";

const NOMBRE_MAX = 150;

interface Props {
    id: string;
}

export default function EditVendedorForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedVendedorIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarVendedorFormValues>({
        initialValues: {
            [VENDEDOR_FIELDS.NOMBRE]: "",
            [VENDEDOR_FIELDS.IS_PRINCIPAL]: false,
            [VENDEDOR_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarVendedorSchema),
    });

    const {
        data: vendedor,
        isLoading: loadingVendedor,
        isError: isVendedorError,
    } = useQuery({
        queryKey: QUERY_KEYS.vendedores.detalle(id),
        queryFn: async () => {
            const res = await obtenerVendedorPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnMount: "always",
    });

    useEffect(() => {
        if (!vendedor) return;
        if (hydratedVendedorIdRef.current === vendedor.id) return;

        form.setValues({
            [VENDEDOR_FIELDS.NOMBRE]: vendedor.nombre,
            [VENDEDOR_FIELDS.IS_PRINCIPAL]: vendedor.isPrincipal,
            [VENDEDOR_FIELDS.ACTIVO]: vendedor.activo,
        });
        form.resetDirty();
        hydratedVendedorIdRef.current = vendedor.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [vendedor]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [vendedor]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarVendedorFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.vendedores.all,
                });
                AppNotifier.success({
                    message: "Vendedor actualizado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarVendedorFormValues) {
        await execute(() => actualizarVendedorAction(id, values));
    }

    if (loadingVendedor) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isVendedorError || !vendedor) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                Error al cargar datos del vendedor.
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
                    <TextInput
                        label="Nombre"
                        placeholder="Nombre del vendedor"
                        maxLength={NOMBRE_MAX}
                        key={form.key(VENDEDOR_FIELDS.NOMBRE)}
                        {...form.getInputProps(VENDEDOR_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Divider className="w-full" />
                <Grid.Col span={12}>
                    <Checkbox
                        label="Principal"
                        key={form.key(VENDEDOR_FIELDS.IS_PRINCIPAL)}
                        disabled={vendedor.isPrincipal}
                        {...form.getInputProps(VENDEDOR_FIELDS.IS_PRINCIPAL, {
                            type: "checkbox",
                        })}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <Checkbox
                        label="Activo"
                        key={form.key(VENDEDOR_FIELDS.ACTIVO)}
                        disabled={vendedor.isPrincipal}
                        {...form.getInputProps(VENDEDOR_FIELDS.ACTIVO, {
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
