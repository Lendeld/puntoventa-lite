"use client";

import ErrorDataAlert from "@/components/ui/ErrorDataAlert";
import { TipoIdentificacionSelect } from "@/components/ui/selects/TipoIdentificacionSelect";
import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarNegocioAction, subirLogoNegocioAction } from "@lib/actions/configuracion.actions";
import { NEGOCIO_FIELDS } from "@lib/constants/configuracion.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useDeploymentMode } from "@lib/hooks/useDeploymentMode";
import { actualizarNegocioSchema } from "@lib/schemas/configuracion.schema";
import type {
    ActualizarNegocioFormValues,
    NegocioDto,
} from "@lib/types/configuracion.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Avatar,
    Badge,
    Button,
    Divider,
    FileButton,
    Grid,
    Group,
    Loader,
    NumberInput,
    Paper,
    ScrollArea,
    Skeleton,
    Stack,
    Text,
    Textarea,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconAlertCircle, IconDeviceFloppy, IconUpload } from "@tabler/icons-react";
import { useEffect, useRef, useState } from "react";

interface NegocioPanelProps {
    puedeEditarNegocio: boolean;
    negocio: NegocioDto | undefined;
    loadingNegocio: boolean;
    isNegocioError: boolean;
    onSaveSuccess: () => Promise<void>;
}

// Panel cohesivo de edición con un solo formulario y preview de logo.
// react-doctor-disable-next-line react-doctor/no-giant-component
export function NegocioPanel({
    puedeEditarNegocio,
    negocio,
    loadingNegocio,
    isNegocioError,
    onSaveSuccess,
}: NegocioPanelProps) {
    const { esLocalHost } = useDeploymentMode();
    // Datos fiscales del negocio + logo son CloudOnly: backend rechaza la
    // mutation y aquí degradamos a solo lectura.
    const puedeEditar = puedeEditarNegocio && !esLocalHost;
    const form = useForm<ActualizarNegocioFormValues>({
        initialValues: {
            [NEGOCIO_FIELDS.NOMBRE]: "",
            [NEGOCIO_FIELDS.NOMBRE_COMERCIAL]: "",
            [NEGOCIO_FIELDS.DIRECCION]: "",
            [NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID]: "",
            [NEGOCIO_FIELDS.IDENTIFICACION]: "",
            [NEGOCIO_FIELDS.TELEFONO]: "",
            [NEGOCIO_FIELDS.CORREO]: "",
            [NEGOCIO_FIELDS.APLICA_VENDEDORES]: false,
            [NEGOCIO_FIELDS.APLICA_CAJAS]: false,
            [NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO]: 500,
        },
        validate: zodResolver(actualizarNegocioSchema),
    });

    useEffect(() => {
        if (!negocio) return;

        form.setValues({
            [NEGOCIO_FIELDS.NOMBRE]: negocio.nombre,
            [NEGOCIO_FIELDS.NOMBRE_COMERCIAL]: negocio.nombreComercial ?? "",
            [NEGOCIO_FIELDS.DIRECCION]: negocio.direccion,
            [NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID]: negocio.tipoIdentificacionCodigo,
            [NEGOCIO_FIELDS.IDENTIFICACION]: negocio.identificacion,
            [NEGOCIO_FIELDS.TELEFONO]: negocio.telefono ?? "",
            [NEGOCIO_FIELDS.CORREO]: negocio.correo,
            // APLICA_VENDEDORES y APLICA_CAJAS viven en ConfiguracionAvanzadaPanel;
            // los arrastramos del valor cargado para no machacarlos al guardar.
            [NEGOCIO_FIELDS.APLICA_VENDEDORES]: negocio.aplicaVendedores,
            [NEGOCIO_FIELDS.APLICA_CAJAS]: negocio.aplicaCajas,
            [NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO]: negocio.tipoCambioPredeterminado,
        });
        form.resetDirty();
    // form.setValues/resetDirty de Mantine no son estables; deps solo [negocio]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [negocio]);

    const [logoPreview, setLogoPreview] = useState<string | null>(null);
    const [uploadingLogo, setUploadingLogo] = useState(false);
    const resetLogoRef = useRef<() => void>(null);

    // logoPreview es editable (el usuario sube un logo nuevo); se sincroniza con
    // el logoUrl del server cuando carga/cambia. No es derivable en render.
    // react-doctor-disable-next-line react-doctor/no-derived-state
    useEffect(() => {
        setLogoPreview(negocio?.logoUrl ?? null);
    }, [negocio?.logoUrl]);

    async function handleLogoChange(file: File | null) {
        if (!file) return;
        setUploadingLogo(true);
        const preview = URL.createObjectURL(file);
        setLogoPreview(preview);
        const result = await subirLogoNegocioAction(file);
        setUploadingLogo(false);
        if (result.errors) {
            setLogoPreview(negocio?.logoUrl ?? null);
            AppNotifier.error({ message: Object.values(result.errors)[0] ?? "Error al subir logo." });
            resetLogoRef.current?.();
            return;
        }
        await onSaveSuccess();
        AppNotifier.success({ message: "Logo actualizado exitosamente." });
    }

    const {
        execute,
        loading: saving,
        error,
        setError,
    } = useActionHandler<ActualizarNegocioFormValues>({
        form,
        onSuccess: async () => {
            await onSaveSuccess();
            AppNotifier.success({
                message: "Negocio actualizado exitosamente.",
            });
        },
    });

    async function handleSubmit(values: ActualizarNegocioFormValues) {
        if (!negocio) return;
        await execute(() => actualizarNegocioAction(negocio.id, values));
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
                            Información del negocio
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
                        onClick={() => void form.onSubmit(handleSubmit)()}
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

            <ScrollArea
                className="flex-1 min-h-0 min-w-0"
                scrollbarSize={6}
            >
                <Stack p="lg">
                    {loadingNegocio && (
                        <Skeleton className="h-[calc(100dvh-360px)]" />
                    )}

                    {!loadingNegocio && isNegocioError && (
                        <ErrorDataAlert
                            message="Error al cargar datos del negocio."
                            height="h-[calc(100dvh-360px)]"
                        />
                    )}

                    {!loadingNegocio && !isNegocioError && negocio && (
                        <form
                            onSubmit={form.onSubmit(
                                handleSubmit,
                                () => setError(null),
                            )}
                            noValidate
                        >
                            <Stack gap="lg">
                                <Stack gap="sm">
                                    <Text fw={600}>Logo del negocio</Text>
                                    <Group gap="md" align="center">
                                        <Avatar
                                            src={logoPreview}
                                            size={80}
                                            radius="md"
                                            alt="Logo del negocio"
                                        />
                                        <Stack gap={4}>
                                            <FileButton
                                                resetRef={resetLogoRef}
                                                onChange={handleLogoChange}
                                                accept="image/jpeg,image/png,image/webp"
                                                disabled={!puedeEditar || uploadingLogo}
                                            >
                                                {(props) => (
                                                    <Button
                                                        {...props}
                                                        variant="light"
                                                        size="xs"
                                                        leftSection={uploadingLogo ? <Loader size={12} /> : <IconUpload size={14} />}
                                                        disabled={!puedeEditar || uploadingLogo}
                                                    >
                                                        {uploadingLogo ? "Subiendo..." : "Cambiar logo"}
                                                    </Button>
                                                )}
                                            </FileButton>
                                            <Text size="xs" c="dimmed">
                                                JPEG, PNG o WebP
                                            </Text>
                                        </Stack>
                                    </Group>
                                </Stack>
                                <Divider />
                                <Grid gap="md">
                                    <Grid.Col span={{ md: 6 }}>
                                        <TextInput
                                            label="Nombre"
                                            placeholder="Nombre legal del negocio"
                                            required
                                            maxLength={100}
                                            key={form.key(NEGOCIO_FIELDS.NOMBRE)}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.NOMBRE,
                                            )}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ md: 6 }}>
                                        <TextInput
                                            label="Nombre comercial"
                                            placeholder="Nombre visible en reportes"
                                            maxLength={80}
                                            key={form.key(
                                                NEGOCIO_FIELDS.NOMBRE_COMERCIAL,
                                            )}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.NOMBRE_COMERCIAL,
                                            )}
                                        />
                                    </Grid.Col>

                                    <Grid.Col span={{ md: 6 }}>
                                        <TipoIdentificacionSelect
                                            label="Tipo de identificación"
                                            placeholder="Selecciona tipo"
                                            selectedValue={
                                                form.values[
                                                    NEGOCIO_FIELDS
                                                        .TIPO_IDENTIFICACION_ID
                                                ]
                                            }
                                            key={form.key(
                                                NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID,
                                            )}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID,
                                            )}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ md: 6 }}>
                                        <TextInput
                                            label="Identificación"
                                            placeholder="Número de identificación"
                                            maxLength={20}
                                            key={form.key(
                                                NEGOCIO_FIELDS.IDENTIFICACION,
                                            )}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.IDENTIFICACION,
                                            )}
                                        />
                                    </Grid.Col>

                                    <Grid.Col span={{ md: 6 }}>
                                        <TextInput
                                            label="Teléfono"
                                            placeholder="+506 0000 0000"
                                            maxLength={20}
                                            key={form.key(
                                                NEGOCIO_FIELDS.TELEFONO,
                                            )}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.TELEFONO,
                                            )}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ md: 6 }}>
                                        <TextInput
                                            label="Correo"
                                            maxLength={160}
                                            placeholder="correo@negocio.com"
                                            key={form.key(
                                                NEGOCIO_FIELDS.CORREO,
                                            )}
                                            disabled={!puedeEditar}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.CORREO,
                                            )}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ md: 6 }}>
                                        <NumberInput
                                            label="Tipo de cambio predeterminado"
                                            description="Se usa como valor inicial al facturar."
                                            min={0}
                                            decimalScale={5}
                                            disabled={!puedeEditar}
                                            key={form.key(
                                                NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO,
                                            )}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO,
                                            )}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ md: 12 }}>
                                        <Textarea
                                            label="Dirección"
                                            placeholder="Dirección del negocio"
                                            required
                                            key={form.key(
                                                NEGOCIO_FIELDS.DIRECCION,
                                            )}
                                            disabled={!puedeEditar}
                                            autosize
                                            minRows={3}
                                            maxLength={255}
                                            {...form.getInputProps(
                                                NEGOCIO_FIELDS.DIRECCION,
                                            )}
                                        />
                                    </Grid.Col>
                                </Grid>
                                {!puedeEditar && (
                                    <>
                                        <Divider />
                                        <Text size="sm" c="dimmed">
                                            Solo lectura. No tienes permiso
                                            para editar el negocio.
                                        </Text>
                                    </>
                                )}
                            </Stack>
                        </form>
                    )}
                </Stack>
            </ScrollArea>
        </Paper>
    );
}
