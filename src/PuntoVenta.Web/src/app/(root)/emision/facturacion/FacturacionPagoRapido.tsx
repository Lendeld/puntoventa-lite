"use client";

import { useMediosPagoActivosQuery } from "@lib/hooks/useMediosPagoActivosQuery";
import type { DocumentoVentaPagoForm } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    Button,
    Card,
    Group,
    Grid,
    HoverCard,
    NumberInput,
    SimpleGrid,
    Stack,
    Text,
    TextInput,
    ThemeIcon,
    UnstyledButton,
} from "@mantine/core";
import { IconInfoCircle } from "@tabler/icons-react";

interface Props {
    pago: DocumentoVentaPagoForm;
    monedaDocumento: string;
    total: number;
    disabled: boolean;
    getFieldError: (path: string) => string | null;
    onUpdatePago: (field: keyof DocumentoVentaPagoForm, value: unknown) => void;
}

// Billetes comunes por moneda para el pago rápido en efectivo.
function denominacionesPorMoneda(moneda: string): number[] {
    return moneda.trim().toUpperCase() === "USD"
        ? [5, 10, 20, 50, 100]
        : [1000, 2000, 5000, 10000, 20000];
}

function etiquetaDenominacion(monto: number, moneda: string): string {
    const simbolo = moneda.trim().toUpperCase() === "USD" ? "$" : "₡";
    return `${simbolo}${monto.toLocaleString("en-US")}`;
}

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

export function FacturacionPagoRapido({
    pago,
    monedaDocumento,
    total,
    disabled,
    getFieldError,
    onUpdatePago,
}: Props) {
    const denominaciones = denominacionesPorMoneda(monedaDocumento);
    const { data: mediosPago = [], isLoading } = useMediosPagoActivosQuery();
    const columnasBase = Math.max(1, Math.min(mediosPago.length, 2));
    const columnasDesktop = Math.max(1, Math.min(mediosPago.length, 4));
    const montoEntregadoActual = Number(pago.MontoEntregado) || 0;
    const medioPagoSeleccionado = mediosPago.find(
        (medio) => medio.codigo === pago.MedioPagoCodigo,
    );
    const mostrarReferencia = requiereReferencia(
        pago.MedioPagoCodigo,
        medioPagoSeleccionado?.detalle ?? "",
    );

    return (
        <Card radius="lg" p="lg">
            <Stack gap="md">
                <Stack gap="xs">
                    <Group justify="space-between" align="center">
                        <Text fw={600}>Metodo de pago</Text>
                        <HoverCard
                            width={260}
                            position="bottom-start"
                            withArrow
                            shadow="md"
                        >
                            <HoverCard.Target>
                                <ThemeIcon
                                    variant="light"
                                    color="gray"
                                    size="sm"
                                    style={{ cursor: "pointer" }}
                                >
                                    <IconInfoCircle size={14} />
                                </ThemeIcon>
                            </HoverCard.Target>
                            <HoverCard.Dropdown>
                                <Text size="sm">
                                    Usa un solo medio de pago en la moneda del
                                    documento. Si ocupas mezclar monedas o
                                    metodos, cambia a detallado.
                                </Text>
                            </HoverCard.Dropdown>
                        </HoverCard>
                    </Group>
                    <SimpleGrid
                        cols={{ base: columnasBase, md: columnasDesktop }}
                        spacing="sm"
                    >
                        {mediosPago.map((medio) => {
                            const selected =
                                pago.MedioPagoCodigo === medio.codigo;

                            return (
                                <UnstyledButton
                                    key={medio.codigo}
                                    onClick={() =>
                                        onUpdatePago(
                                            "MedioPagoCodigo",
                                            medio.codigo,
                                        )
                                    }
                                    disabled={disabled || isLoading}
                                    className={`rounded-lg border px-3 py-2 text-center transition border-theme ${
                                        selected
                                            ? "bg-theme shadow-sm"
                                            : "bg-theme-surface-2 hover:bg-theme/60"
                                    }`}
                                >
                                    <Text fw={600} size="sm">
                                        {medio.detalle}
                                    </Text>
                                </UnstyledButton>
                            );
                        })}
                    </SimpleGrid>
                    {getFieldError(
                        "DocumentoVenta_Pagos.0.MedioPagoCodigo",
                    ) && (
                        <Text c="red" size="sm">
                            {getFieldError(
                                "DocumentoVenta_Pagos.0.MedioPagoCodigo",
                            )}
                        </Text>
                    )}
                </Stack>

                <Stack gap="xs">
                    <Text size="sm" c="dimmed">
                        Pago rápido
                    </Text>
                    <Group gap="xs">
                        <Button
                            variant="filled"
                            color="accentPV"
                            size="xs"
                            radius="md"
                            onClick={() => onUpdatePago("MontoEntregado", total)}
                            disabled={disabled || total <= 0}
                        >
                            Exacto
                        </Button>
                        {denominaciones.map((monto) => (
                            <Button
                                key={monto}
                                variant="default"
                                size="xs"
                                radius="md"
                                onClick={() =>
                                    onUpdatePago(
                                        "MontoEntregado",
                                        montoEntregadoActual + monto,
                                    )
                                }
                                disabled={disabled}
                            >
                                +{etiquetaDenominacion(monto, monedaDocumento)}
                            </Button>
                        ))}
                        <Button
                            variant="default"
                            color="red"
                            size="xs"
                            radius="md"
                            onClick={() => onUpdatePago("MontoEntregado", 0)}
                            disabled={disabled || montoEntregadoActual <= 0}
                        >
                            Limpiar
                        </Button>
                    </Group>
                </Stack>

                <Grid align="end">
                    <Grid.Col span={{ base: 12, md: 7 }}>
                        <NumberInput
                            label={`Monto entregado (${monedaDocumento})`}
                            value={pago.MontoEntregado}
                            onChange={(value) =>
                                onUpdatePago(
                                    "MontoEntregado",
                                    typeof value === "number"
                                        ? value
                                        : parseFloat(String(value)) || 0,
                                )
                            }
                            error={
                                getFieldError(
                                    "DocumentoVenta_Pagos.0.MontoEntregado",
                                ) ?? undefined
                            }
                            min={0}
                            decimalScale={5}
                            fixedDecimalScale={false}
                            disabled={disabled}
                        />
                    </Grid.Col>
                    {mostrarReferencia && (
                        <Grid.Col span={{ base: 12, md: 5 }}>
                            <TextInput
                                label="Referencia"
                                value={pago.Referencia}
                                onChange={(event) =>
                                    onUpdatePago(
                                        "Referencia",
                                        event.currentTarget.value,
                                    )
                                }
                                error={
                                    getFieldError(
                                        "DocumentoVenta_Pagos.0.Referencia",
                                    ) ?? undefined
                                }
                                disabled={disabled}
                            />
                        </Grid.Col>
                    )}
                </Grid>

                <Group justify="space-between" align="flex-start" wrap="wrap">
                    <Stack gap={2}>
                        <Text size="sm" c="dimmed">
                            Vuelto
                        </Text>
                        <Text fw={700}>
                            {formatMonedaPorCodigo(
                                pago.MontoVueltoMonedaPago,
                                monedaDocumento,
                            )}
                        </Text>
                    </Stack>
                    <Stack gap={2} align="flex-end">
                        <Text size="sm" c="dimmed">
                            Tipo cambio aplicado
                        </Text>
                        <Text fw={700}>
                            {pago.TipoCambioAplicado.toLocaleString("en-US", {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 5,
                            })}
                        </Text>
                    </Stack>
                </Group>
            </Stack>
        </Card>
    );
}
