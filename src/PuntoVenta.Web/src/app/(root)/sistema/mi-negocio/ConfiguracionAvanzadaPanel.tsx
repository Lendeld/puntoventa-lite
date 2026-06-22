"use client";

import ErrorDataAlert from "@/components/ui/ErrorDataAlert";
import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarNegocioAction } from "@lib/actions/configuracion.actions";
import { NEGOCIO_FIELDS } from "@lib/constants/configuracion.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useDeploymentMode } from "@lib/hooks/useDeploymentMode";
import type {
    ActualizarNegocioFormValues,
    NegocioDto,
} from "@lib/types/configuracion.types";
import {
    Alert,
    Badge,
    Button,
    Group,
    Paper,
    ScrollArea,
    Skeleton,
    Stack,
    Switch,
    Text,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconAlertCircle, IconDeviceFloppy } from "@tabler/icons-react";
import { useEffect } from "react";

interface ConfiguracionAvanzadaPanelProps {
    puedeEditarNegocio: boolean;
    negocio: NegocioDto | undefined;
    loadingNegocio: boolean;
    isNegocioError: boolean;
    onSaveSuccess: () => Promise<void>;
}

export function ConfiguracionAvanzadaPanel({
    puedeEditarNegocio,
    negocio,
    loadingNegocio,
    isNegocioError,
    onSaveSuccess,
}: ConfiguracionAvanzadaPanelProps) {
    const { esLocalHost } = useDeploymentMode();
    const puedeEditar = puedeEditarNegocio && !esLocalHost;

    const form = useForm<{
        [NEGOCIO_FIELDS.APLICA_VENDEDORES]: boolean;
        [NEGOCIO_FIELDS.APLICA_CAJAS]: boolean;
    }>({
        initialValues: {
            [NEGOCIO_FIELDS.APLICA_VENDEDORES]: false,
            [NEGOCIO_FIELDS.APLICA_CAJAS]: false,
        },
    });

    useEffect(() => {
        if (!negocio) return;
        form.setValues({
            [NEGOCIO_FIELDS.APLICA_VENDEDORES]: negocio.aplicaVendedores,
            [NEGOCIO_FIELDS.APLICA_CAJAS]: negocio.aplicaCajas,
        });
        form.resetDirty();
    // form.setValues/resetDirty de Mantine no son estables; deps solo [negocio]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [negocio]);

    const {
        execute,
        loading: saving,
        error,
    } = useActionHandler({
        form,
        onSuccess: async () => {
            await onSaveSuccess();
            AppNotifier.success({
                message: "Configuración actualizada exitosamente.",
            });
        },
    });

    async function handleSubmit() {
        if (!negocio) return;
        const payload: ActualizarNegocioFormValues = {
            [NEGOCIO_FIELDS.NOMBRE]: negocio.nombre,
            [NEGOCIO_FIELDS.NOMBRE_COMERCIAL]: negocio.nombreComercial ?? "",
            [NEGOCIO_FIELDS.DIRECCION]: negocio.direccion,
            [NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID]: negocio.tipoIdentificacionCodigo,
            [NEGOCIO_FIELDS.IDENTIFICACION]: negocio.identificacion,
            [NEGOCIO_FIELDS.TELEFONO]: negocio.telefono ?? "",
            [NEGOCIO_FIELDS.CORREO]: negocio.correo,
            [NEGOCIO_FIELDS.APLICA_VENDEDORES]:
                form.values[NEGOCIO_FIELDS.APLICA_VENDEDORES],
            [NEGOCIO_FIELDS.APLICA_CAJAS]:
                form.values[NEGOCIO_FIELDS.APLICA_CAJAS],
            [NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO]:
                negocio.tipoCambioPredeterminado,
        };
        await execute(() => actualizarNegocioAction(negocio.id, payload));
    }

    return (
        <Paper className="bg-theme-surface h-full flex flex-col">
            {error && (
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    color="red"
                    variant="light"
                    className="mx-5 mt-4"
                >
                    {error}
                </Alert>
            )}

            <Group
                justify="space-between"
                className="w-full border-b border-theme p-5"
            >
                <Stack gap={2} className="min-w-0 flex-1">
                    <Group gap="xs">
                        <Text fw={700} size="lg">
                            Configuración avanzada
                        </Text>
                        {form.isDirty() && (
                            <Badge variant="light" color="yellow">
                                Sin guardar
                            </Badge>
                        )}
                    </Group>
                </Stack>
                <Group>
                    <Button
                        leftSection={<IconDeviceFloppy size={16} />}
                        onClick={() => void handleSubmit()}
                        loading={saving}
                        disabled={
                            !puedeEditar ||
                            !form.isDirty() ||
                            loadingNegocio
                        }
                    >
                        Guardar cambios
                    </Button>
                </Group>
            </Group>

            <ScrollArea className="flex-1 min-h-0 min-w-0" scrollbarSize={6}>
                <Stack p="lg">
                    {loadingNegocio && (
                        <Skeleton className="h-[calc(100dvh-360px)]" />
                    )}

                    {!loadingNegocio && isNegocioError && (
                        <ErrorDataAlert
                            message="Error al cargar la configuración del negocio."
                            height="h-[calc(100dvh-360px)]"
                        />
                    )}

                    {!loadingNegocio && !isNegocioError && negocio && (
                        <Stack gap="lg">
                            <Group
                                justify="space-between"
                                align="center"
                                wrap="nowrap"
                            >
                                <Stack gap={2} className="min-w-0">
                                    <Text fw={600}>Aplica vendedores</Text>
                                    <Text size="sm" c="dimmed">
                                        Permite seleccionar vendedor al
                                        facturar.
                                    </Text>
                                </Stack>
                                <Switch
                                    className="shrink-0"
                                    size="md"
                                    checked={
                                        form.values[
                                            NEGOCIO_FIELDS.APLICA_VENDEDORES
                                        ]
                                    }
                                    onChange={(event) =>
                                        form.setFieldValue(
                                            NEGOCIO_FIELDS.APLICA_VENDEDORES,
                                            event.currentTarget.checked,
                                        )
                                    }
                                    disabled={!puedeEditar}
                                    aria-label="Aplica vendedores"
                                />
                            </Group>
                            <Group
                                justify="space-between"
                                align="center"
                                wrap="nowrap"
                            >
                                <Stack gap={2} className="min-w-0">
                                    <Text fw={600}>Aplica cajas</Text>
                                    <Text size="sm" c="dimmed">
                                        Permite crear múltiples cajas, parear
                                        instalaciones desktop y requerir caja al
                                        facturar.
                                    </Text>
                                </Stack>
                                <Switch
                                    className="shrink-0"
                                    size="md"
                                    checked={
                                        form.values[
                                            NEGOCIO_FIELDS.APLICA_CAJAS
                                        ]
                                    }
                                    onChange={(event) =>
                                        form.setFieldValue(
                                            NEGOCIO_FIELDS.APLICA_CAJAS,
                                            event.currentTarget.checked,
                                        )
                                    }
                                    disabled={!puedeEditar}
                                    aria-label="Aplica cajas"
                                />
                            </Group>
                            {!puedeEditar && (
                                <Text size="sm" c="dimmed">
                                    Solo lectura. No tienes permiso para editar
                                    la configuración del negocio.
                                </Text>
                            )}
                        </Stack>
                    )}
                </Stack>
            </ScrollArea>
        </Paper>
    );
}
