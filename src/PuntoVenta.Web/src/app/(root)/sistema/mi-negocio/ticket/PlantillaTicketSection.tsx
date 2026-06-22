"use client";

import ErrorDataAlert from "@/components/ui/ErrorDataAlert";
import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarNegocioTicketConfigAction } from "@lib/actions/impresion.actions";
import {
    ENCABEZADO_POR_DEFECTO,
    NEGOCIO_TICKET_CONFIG_FIELDS,
} from "@lib/constants/negocio-ticket-config.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarNegocioTicketConfigSchema } from "@lib/schemas/negocio-ticket-config.schema";
import type {
    ActualizarNegocioTicketConfigFormValues,
    ElementoEncabezadoTipo,
    NegocioTicketConfigDto,
} from "@lib/types/impresion.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Badge,
    Button,
    Divider,
    Group,
    Paper,
    SimpleGrid,
    Skeleton,
    Stack,
    Switch,
    Text,
    Textarea,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { IconAlertCircle, IconDeviceFloppy } from "@tabler/icons-react";
import { useEffect } from "react";
import { ConfiguracionesEditor } from "@pages/sistema/mi-negocio/ticket/ConfiguracionesEditor";
import { EncabezadoEditor } from "@pages/sistema/mi-negocio/ticket/EncabezadoEditor";

interface Props {
    config: NegocioTicketConfigDto | undefined;
    loading: boolean;
    isError: boolean;
    puedeEditar: boolean;
    onSaveSuccess: () => Promise<void>;
}

const MENSAJE_PIE_MAX = 240;

export function PlantillaTicketSection({
    config,
    loading,
    isError,
    puedeEditar,
    onSaveSuccess,
}: Props) {
    const form = useForm<ActualizarNegocioTicketConfigFormValues>({
        initialValues: {
            [NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE]: "",
            [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO]: true,
            [NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO]: false,
            [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS]: true,
            [NEGOCIO_TICKET_CONFIG_FIELDS.CONFIGURACIONES]: [],
            [NEGOCIO_TICKET_CONFIG_FIELDS.ELEMENTOS_ENCABEZADO]: [],
        },
        validate: zodResolver(actualizarNegocioTicketConfigSchema),
    });

    useEffect(() => {
        if (!config) return;
        form.setValues({
            [NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE]: config.mensajePie ?? "",
            [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO]: config.mostrarLogo,
            [NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO]:
                config.aplicaCopiaClienteNegocio,
            [NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS]:
                config.mostrarCodigoBarras,
            [NEGOCIO_TICKET_CONFIG_FIELDS.CONFIGURACIONES]:
                config.configuraciones.map((configuracion) => ({
                    _key: crypto.randomUUID(),
                    nombre: configuracion.nombre,
                    destino: configuracion.destino,
                    tiposDocumento: configuracion.tiposDocumento,
                    lineas: configuracion.lineas.map((linea) => ({
                        _key: crypto.randomUUID(),
                        texto: linea.texto,
                        alineacion: linea.alineacion,
                        negrita: linea.negrita,
                    })),
                })),
            [NEGOCIO_TICKET_CONFIG_FIELDS.ELEMENTOS_ENCABEZADO]:
                (config.elementosEncabezado.length > 0
                    ? config.elementosEncabezado
                    : ENCABEZADO_POR_DEFECTO.map((e) => ({
                          tipo: e.tipo as ElementoEncabezadoTipo,
                          visible: e.visible,
                          textoLibre: null as string | null,
                      }))
                ).map((el) => ({
                    _key: crypto.randomUUID(),
                    tipo: el.tipo,
                    visible: el.visible,
                    textoLibre: el.textoLibre ?? "",
                })),
        });
        form.resetDirty();
    // form.setValues/resetDirty de Mantine no son estables; deps solo [config]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [config]);

    const { execute, loading: saving, error, setError } =
        useActionHandler<ActualizarNegocioTicketConfigFormValues>({
            form,
            onSuccess: async () => {
                await onSaveSuccess();
                AppNotifier.success({ message: "Plantilla del ticket actualizada." });
            },
        });

    async function handleSubmit(values: ActualizarNegocioTicketConfigFormValues) {
        await execute(() => actualizarNegocioTicketConfigAction(values));
    }

    return (
        <Paper className="bg-theme-surface">
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
                            Plantilla del ticket
                        </Text>
                        {form.isDirty() && (
                            <Badge variant="light" color="yellow">
                                Sin guardar
                            </Badge>
                        )}
                    </Group>
                    <Text size="sm" c="dimmed">
                        Configuración global del contenido del ticket impreso.
                    </Text>
                </Stack>
                <Group>
                    <Button
                        leftSection={<IconDeviceFloppy size={16} />}
                        onClick={() => {
                            form.onSubmit(handleSubmit)();
                        }}
                        loading={saving}
                        disabled={!puedeEditar || !form.isDirty() || loading}
                    >
                        Guardar cambios
                    </Button>
                </Group>
            </Group>

            <Stack p="lg">
                {loading && <Skeleton className="h-40" />}

                {!loading && isError && (
                    <ErrorDataAlert message="No se pudo cargar la plantilla del ticket." />
                )}

                {!loading && !isError && (
                    <form
                        onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
                        noValidate
                    >
                        <Stack gap="md">
                            <Textarea
                                label="Mensaje de pie"
                                description="Se imprime al final del ticket, por ejemplo: 'Gracias por su compra'."
                                placeholder="Mensaje opcional"
                                autosize
                                minRows={2}
                                maxRows={4}
                                maxLength={MENSAJE_PIE_MAX}
                                key={form.key(NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE)}
                                disabled={!puedeEditar}
                                {...form.getInputProps(NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE)}
                            />

                            <SimpleGrid
                                cols={{ base: 1, sm: 2 }}
                                spacing="md"
                                verticalSpacing="lg"
                            >
                                <Switch
                                    label="Mostrar logo del negocio"
                                    description="Si está activo y el negocio tiene logo, se imprime en el encabezado."
                                    key={form.key(NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO)}
                                    disabled={!puedeEditar}
                                    checked={form.values[NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO]}
                                    {...form.getInputProps(NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO, {
                                        type: "checkbox",
                                    })}
                                />

                                <Switch
                                    label="Aplica copia cliente/negocio"
                                    description="Imprime 2 copias del ticket: una rotulada 'Cliente' y otra 'Negocio' al pie."
                                    key={form.key(NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO)}
                                    disabled={!puedeEditar}
                                    checked={form.values[NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO]}
                                    {...form.getInputProps(NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO, {
                                        type: "checkbox",
                                    })}
                                />

                                <Switch
                                    label="Mostrar código de barras"
                                    description="Imprime el código de barras del consecutivo al pie del ticket y del PDF."
                                    key={form.key(NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS)}
                                    disabled={!puedeEditar}
                                    checked={form.values[NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS]}
                                    {...form.getInputProps(NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS, {
                                        type: "checkbox",
                                    })}
                                />
                            </SimpleGrid>

                            <Divider />

                            <EncabezadoEditor form={form} puedeEditar={puedeEditar} />

                            <Divider />

                            <ConfiguracionesEditor form={form} puedeEditar={puedeEditar} />
                        </Stack>
                    </form>
                )}
            </Stack>
        </Paper>
    );
}
