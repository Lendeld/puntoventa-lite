"use client";

import { useMediosPagoActivosQuery } from "@lib/hooks/useMediosPagoActivosQuery";
import type { DocumentoVentaPagoForm } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    ActionIcon,
    Button,
    Card,
    Grid,
    Group,
    NumberInput,
    Select,
    Stack,
    Text,
    TextInput,
} from "@mantine/core";
import { IconPlus, IconTrash } from "@tabler/icons-react";

interface Props {
    pagos: DocumentoVentaPagoForm[];
    monedaDocumento: string;
    disabled: boolean;
    pagosError?: string | null;
    onAgregarPago: (medioPagoCodigo?: string) => void;
    onUpdatePago: (
        index: number,
        field: keyof DocumentoVentaPagoForm,
        value: unknown,
    ) => void;
    onRemovePago: (index: number) => void;
    getFieldError: (path: string) => string | null;
}

const MONEDAS = [
    { value: "CRC", label: "Colones (CRC)" },
    { value: "USD", label: "Dólares (USD)" },
];

function requiereReferencia(medioPagoCodigo: string, medioPagoDetalle: string) {
    const codigo = medioPagoCodigo.trim().toUpperCase();
    const detalle = medioPagoDetalle.trim().toUpperCase();

    return (
        codigo.includes("TARJ") ||
        codigo.includes("TRAN") ||
        detalle.includes("TARJ") ||
        detalle.includes("TRANSFER")
    );
}

function resolveDefaultMedioPagoCodigo(
    mediosPago: Array<{ codigo: string; detalle: string }>,
) {
    const medioEfectivo = mediosPago.find((medio) => {
        const codigo = medio.codigo.trim().toUpperCase();
        const detalle = medio.detalle.trim().toUpperCase();

        return (
            codigo.includes("EFE") || detalle.includes("EFECTIVO")
        );
    });

    return medioEfectivo?.codigo ?? mediosPago[0]?.codigo;
}

export function FacturacionPagosEditor({
    pagos,
    monedaDocumento,
    disabled,
    pagosError,
    onAgregarPago,
    onUpdatePago,
    onRemovePago,
    getFieldError,
}: Props) {
    const { data: mediosPago = [], isLoading } = useMediosPagoActivosQuery();

    return (
        <Card
            radius="lg"
            p="lg"
           
        >
            <Stack gap="md">
                <Group justify="space-between">
                    <Stack gap={2}>
                        <Text fw={700}>Pagos</Text>
                        <Text size="sm" c="dimmed">
                            Registra moneda, monto entregado y el sistema calcula aplicado y vuelto.
                        </Text>
                    </Stack>
                    <Button
                        size="xs"
                        variant="light"
                        leftSection={<IconPlus size={14} />}
                        onClick={() =>
                            onAgregarPago(
                                resolveDefaultMedioPagoCodigo(mediosPago),
                            )
                        }
                        disabled={disabled}
                    >
                        Agregar pago
                    </Button>
                </Group>

                {pagosError && <Text c="red" size="sm">{pagosError}</Text>}

                {pagos.length === 0 ? (
                    <Text size="sm" c="dimmed">
                        No hay pagos agregados.
                    </Text>
                ) : (
                    pagos.map((pago, index) => (
                        (() => {
                            const medioPagoSeleccionado = mediosPago.find(
                                (medio) => medio.codigo === pago.MedioPagoCodigo,
                            );
                            const mostrarReferencia = requiereReferencia(
                                pago.MedioPagoCodigo,
                                medioPagoSeleccionado?.detalle ?? "",
                            );

                            return (
                                // Inputs 100% controlados por el array (value={pago.*}) y el
                                // index es el path del form; no hay estado interno que se
                                // desincronice al remover. Un id estable filtraría al payload
                                // de cobro (ruta crítica), no vale el riesgo por un key.
                                // react-doctor-disable-next-line react-doctor/no-array-index-key, react-doctor/no-array-index-as-key
                                <Card
                                    key={`pago-${index}`}
                                    radius="md"
                                    p="md"
                                    className="bg-theme-surface-2 border border-theme"
                                >
                                    <Stack gap="sm">
                                        <Grid align="end">
                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <Select
                                            label="Medio de pago"
                                            data={mediosPago.map((medio) => ({
                                                value: medio.codigo,
                                                label: medio.detalle,
                                            }))}
                                            value={pago.MedioPagoCodigo}
                                            onChange={(value) =>
                                                onUpdatePago(
                                                    index,
                                                    "MedioPagoCodigo",
                                                    value ?? "",
                                                )
                                            }
                                            error={
                                                getFieldError(
                                                    `DocumentoVenta_Pagos.${index}.MedioPagoCodigo`,
                                                ) ?? undefined
                                            }
                                            disabled={isLoading || disabled}
                                            allowDeselect={false}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 3 }}>
                                        <Select
                                            label="Moneda del pago"
                                            data={MONEDAS}
                                            value={pago.MonedaCodigo}
                                            onChange={(value) =>
                                                onUpdatePago(
                                                    index,
                                                    "MonedaCodigo",
                                                    value ?? monedaDocumento,
                                                )
                                            }
                                            error={
                                                getFieldError(
                                                    `DocumentoVenta_Pagos.${index}.MonedaCodigo`,
                                                ) ?? undefined
                                            }
                                            disabled={disabled}
                                            allowDeselect={false}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 3 }}>
                                        <NumberInput
                                            label="Monto entregado"
                                            value={pago.MontoEntregado}
                                            onChange={(value) =>
                                                onUpdatePago(
                                                    index,
                                                    "MontoEntregado",
                                                    typeof value === "number"
                                                        ? value
                                                        : parseFloat(
                                                              String(value),
                                                          ) || 0,
                                                )
                                            }
                                            error={
                                                getFieldError(
                                                    `DocumentoVenta_Pagos.${index}.MontoEntregado`,
                                                ) ?? undefined
                                            }
                                            min={0}
                                            decimalScale={5}
                                            fixedDecimalScale={false}
                                            disabled={disabled}
                                        />
                                    </Grid.Col>
                                    {mostrarReferencia && (
                                        <Grid.Col span={{ base: 10, md: 2 }}>
                                            <TextInput
                                                label="Referencia"
                                                value={pago.Referencia}
                                                onChange={(event) =>
                                                    onUpdatePago(
                                                        index,
                                                        "Referencia",
                                                        event.currentTarget.value,
                                                    )
                                                }
                                                error={
                                                    getFieldError(
                                                        `DocumentoVenta_Pagos.${index}.Referencia`,
                                                    ) ?? undefined
                                                }
                                                disabled={disabled}
                                            />
                                        </Grid.Col>
                                    )}
                                    <Grid.Col span={{ base: 2, md: 0.5 }}>
                                        <ActionIcon
                                            variant="light"
                                            color="red"
                                            onClick={() => onRemovePago(index)}
                                            disabled={disabled}
                                            aria-label={`Eliminar pago ${index + 1}`}
                                        >
                                            <IconTrash size={16} />
                                        </ActionIcon>
                                    </Grid.Col>
                                        </Grid>

                                        <Grid>
                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <Text size="sm" c="dimmed">
                                            Aplicado en moneda del pago
                                        </Text>
                                        <Text fw={600}>
                                            {formatMonedaPorCodigo(
                                                pago.MontoAplicadoMonedaPago,
                                                pago.MonedaCodigo,
                                            )}
                                        </Text>
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <Text size="sm" c="dimmed">
                                            Aplicado a factura
                                        </Text>
                                        <Text fw={600}>
                                            {formatMonedaPorCodigo(
                                                pago.MontoAplicadoDocumento,
                                                monedaDocumento,
                                            )}
                                        </Text>
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 4 }}>
                                        <Text size="sm" c="dimmed">
                                            Vuelto
                                        </Text>
                                        <Text fw={600}>
                                            {formatMonedaPorCodigo(
                                                pago.MontoVueltoMonedaPago,
                                                pago.MonedaCodigo,
                                            )}
                                        </Text>
                                    </Grid.Col>
                                        </Grid>

                                        <Grid align="end">
                                    <Grid.Col span={{ base: 12, md: 9 }}>
                                        <TextInput
                                            label="Observación"
                                            value={pago.Observacion}
                                            onChange={(event) =>
                                                onUpdatePago(
                                                    index,
                                                    "Observacion",
                                                    event.currentTarget.value,
                                                )
                                            }
                                            error={
                                                getFieldError(
                                                    `DocumentoVenta_Pagos.${index}.Observacion`,
                                                ) ?? undefined
                                            }
                                            disabled={disabled}
                                        />
                                    </Grid.Col>
                                    <Grid.Col span={{ base: 12, md: 3 }}>
                                        <Text size="sm" c="dimmed">
                                            Tipo cambio aplicado
                                        </Text>
                                        <Text fw={600}>
                                            {pago.TipoCambioAplicado.toLocaleString(
                                                "en-US",
                                                {
                                                    minimumFractionDigits: 2,
                                                    maximumFractionDigits: 5,
                                                },
                                            )}
                                        </Text>
                                    </Grid.Col>
                                        </Grid>
                                    </Stack>
                                </Card>
                            );
                        })()
                    ))
                )}
            </Stack>
        </Card>
    );
}
