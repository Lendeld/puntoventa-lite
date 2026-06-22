"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarRolAction } from "@lib/actions/roles.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ROL_FIELDS } from "@lib/constants/roles.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarRolSchema } from "@lib/schemas/roles.schema";
import { obtenerRolPorIdService } from "@lib/services/roles.service";
import type { ActualizarRolFormValues } from "@lib/types/roles.types";
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

const NOMBRE_MAX = 100;
const DESCRIPCION_MAX = 500;

interface Props {
    id: string;
}

export default function EditRolForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedRolIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarRolFormValues>({
        initialValues: {
            [ROL_FIELDS.NOMBRE]: "",
            [ROL_FIELDS.DESCRIPCION]: "",
            [ROL_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarRolSchema),
    });

    const {
        data: rol,
        isLoading: loadingRol,
        isError: isRolError,
    } = useQuery({
        queryKey: QUERY_KEYS.roles.detalle(id),
        queryFn: async () => {
            const res = await obtenerRolPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    useEffect(() => {
        if (!rol) return;
        if (hydratedRolIdRef.current === rol.id) return;

        form.setValues({
            [ROL_FIELDS.NOMBRE]: rol.nombre,
            [ROL_FIELDS.DESCRIPCION]: rol.descripcion ?? "",
            [ROL_FIELDS.ACTIVO]: rol.activo,
        });
        form.resetDirty();
        hydratedRolIdRef.current = rol.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [rol]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [rol]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarRolFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.roles.all,
                });
                AppNotifier.success({
                    message: "Rol actualizado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarRolFormValues) {
        await execute(() => actualizarRolAction(id, values));
    }

    if (loadingRol) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isRolError || !rol) {
        return (
            <Alert
                icon={<IconAlertCircle size={16} />}
                variant="light"
                color="red"
            >
                Error al cargar datos del rol.
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
                {!rol.isPrincipal && (
                    <>
                        <Divider className="w-full" />
                        <Grid.Col span={12}>
                            <Checkbox
                                label="Activo"
                                key={form.key(ROL_FIELDS.ACTIVO)}
                                {...form.getInputProps(ROL_FIELDS.ACTIVO, {
                                    type: "checkbox",
                                })}
                            />
                        </Grid.Col>
                    </>
                )}
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
