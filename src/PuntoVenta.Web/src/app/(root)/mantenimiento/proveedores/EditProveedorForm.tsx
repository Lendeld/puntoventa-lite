"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarProveedorAction } from "@lib/actions/proveedores.actions";
import { PROVEEDOR_FIELDS, PROVEEDOR_MAX } from "@lib/constants/proveedores.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarProveedorSchema } from "@lib/schemas/proveedores.schema";
import { obtenerProveedorPorIdService } from "@lib/services/proveedores.service";
import type { ActualizarProveedorFormValues } from "@lib/types/proveedores.types";
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
    Textarea,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle } from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";

interface Props {
    id: string;
}

export default function EditProveedorForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedProveedorIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarProveedorFormValues>({
        initialValues: {
            [PROVEEDOR_FIELDS.NOMBRE]: "",
            [PROVEEDOR_FIELDS.CORREO]: "",
            [PROVEEDOR_FIELDS.TELEFONO]: "",
            [PROVEEDOR_FIELDS.OBSERVACION]: "",
            [PROVEEDOR_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarProveedorSchema),
    });

    const {
        data: proveedor,
        isLoading: loadingProveedor,
        isError: isProveedorError,
    } = useQuery({
        queryKey: QUERY_KEYS.proveedores.detalle(id),
        queryFn: async () => {
            const res = await obtenerProveedorPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnMount: "always",
    });

    useEffect(() => {
        if (!proveedor) return;
        if (hydratedProveedorIdRef.current === proveedor.id) return;

        form.setValues({
            [PROVEEDOR_FIELDS.NOMBRE]: proveedor.nombre,
            [PROVEEDOR_FIELDS.CORREO]: proveedor.correo ?? "",
            [PROVEEDOR_FIELDS.TELEFONO]: proveedor.telefono ?? "",
            [PROVEEDOR_FIELDS.OBSERVACION]: proveedor.observacion ?? "",
            [PROVEEDOR_FIELDS.ACTIVO]: proveedor.activo,
        });
        form.resetDirty();
        hydratedProveedorIdRef.current = proveedor.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [proveedor]
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [proveedor]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarProveedorFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.proveedores.all,
                });
                AppNotifier.success({
                    message: "Proveedor actualizado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarProveedorFormValues) {
        await execute(() => actualizarProveedorAction(id, values));
    }

    if (loadingProveedor) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isProveedorError || !proveedor) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                Error al cargar datos del proveedor.
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
                        placeholder="Nombre del proveedor"
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
                <Divider className="w-full" />
                <Grid.Col span={12}>
                    <Checkbox
                        label="Activo"
                        key={form.key(PROVEEDOR_FIELDS.ACTIVO)}
                        {...form.getInputProps(PROVEEDOR_FIELDS.ACTIVO, {
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
