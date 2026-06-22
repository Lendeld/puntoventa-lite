"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarCategoriaAction } from "@lib/actions/categorias.actions";
import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarCategoriaSchema } from "@lib/schemas/categorias.schema";
import { obtenerCategoriaPorIdService } from "@lib/services/categorias.service";
import type { ActualizarCategoriaFormValues } from "@lib/types/categorias.types";
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

const NOMBRE_MAX = 150;
const DESCRIPCION_MAX = 255;

interface Props {
    id: string;
}

export default function EditCategoriaForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedCategoriaIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarCategoriaFormValues>({
        initialValues: {
            [CATEGORIA_FIELDS.NOMBRE]: "",
            [CATEGORIA_FIELDS.DESCRIPCION]: "",
            [CATEGORIA_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarCategoriaSchema),
    });

    const {
        data: categoria,
        isLoading: loadingCategoria,
        isError: isCategoriaError,
    } = useQuery({
        queryKey: QUERY_KEYS.categorias.detalle(id),
        queryFn: async () => {
            const res = await obtenerCategoriaPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 0,
        refetchOnMount: "always",
    });

    useEffect(() => {
        if (!categoria) return;
        if (hydratedCategoriaIdRef.current === categoria.id) return;

        form.setValues({
            [CATEGORIA_FIELDS.NOMBRE]: categoria.nombre,
            [CATEGORIA_FIELDS.DESCRIPCION]: categoria.descripcion ?? "",
            [CATEGORIA_FIELDS.ACTIVO]: categoria.activo,
        });
        form.resetDirty();
        hydratedCategoriaIdRef.current = categoria.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [categoria]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [categoria]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarCategoriaFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.categorias.all,
                });
                AppNotifier.success({
                    message: "Categoría actualizada exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarCategoriaFormValues) {
        await execute(() => actualizarCategoriaAction(id, values));
    }

    if (loadingCategoria) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isCategoriaError || !categoria) {
        return (
            <Alert icon={<IconAlertCircle size={16} />} variant="light" color="red">
                Error al cargar datos de la categoría.
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
                        placeholder="Nombre de la categoría"
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
                <Divider className="w-full" />
                <Grid.Col span={12}>
                    <Checkbox
                        label="Activo"
                        key={form.key(CATEGORIA_FIELDS.ACTIVO)}
                        {...form.getInputProps(CATEGORIA_FIELDS.ACTIVO, {
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
