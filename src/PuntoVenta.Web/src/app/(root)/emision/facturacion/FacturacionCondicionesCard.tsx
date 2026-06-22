"use client";

import {
    CONDICIONES_VENTA_CREDITO,
    esCondicionVentaOcultaEnFacturacion,
    VENTA_FIELDS,
} from "@lib/constants/ventas.constants";
import { useCondicionesVentaActivasQuery } from "@lib/hooks/useCondicionesVentaActivasQuery";
import type { FacturacionDocumentoCardProps } from "@pages/emision/facturacion/FacturacionDocumentoCard";
import { Card, Group, NumberInput, SegmentedControl, Select, Stack, Text, ThemeIcon } from "@mantine/core";
import { IconReceiptTax } from "@tabler/icons-react";

const MONEDAS = [
    { value: "CRC", label: "Colones" },
    { value: "USD", label: "Dolares" },
] as const;

export function FacturacionCondicionesCard({
    values,
    errors,
    disabled,
    onFieldChange,
}: FacturacionDocumentoCardProps) {
    const { data: condiciones = [], isLoading } = useCondicionesVentaActivasQuery();
    const condicionVentaCodigo = String(values[VENTA_FIELDS.CONDICION_VENTA_CODIGO] ?? "");
    const esCredito = (CONDICIONES_VENTA_CREDITO as readonly string[]).includes(condicionVentaCodigo);
    const monedaCodigo = String(values[VENTA_FIELDS.MONEDA_CODIGO] ?? "CRC").toUpperCase();

    return (
        <Card radius="lg" p="md" className="h-full">
            <Stack gap="sm">
                <Group gap="xs" align="center">
                    <ThemeIcon variant="light" color="accentPV" size="sm">
                        <IconReceiptTax size={15} />
                    </ThemeIcon>
                    <Text fw={700}>Condiciones</Text>
                </Group>
                <Stack gap="sm">
                    <Select
                        label="Condicion de venta"
                        size="sm"
                        placeholder="Selecciona una condicion"
                        data={condiciones.flatMap((condicion) =>
                            !esCondicionVentaOcultaEnFacturacion(condicion.codigo) ||
                            condicion.codigo === condicionVentaCodigo
                                ? [{ value: condicion.codigo, label: condicion.detalle }]
                                : [],
                        )}
                        value={condicionVentaCodigo}
                        onChange={(value) =>
                            onFieldChange(VENTA_FIELDS.CONDICION_VENTA_CODIGO, value ?? "")
                        }
                        error={errors[VENTA_FIELDS.CONDICION_VENTA_CODIGO]}
                        disabled={isLoading || disabled}
                        allowDeselect={false}
                    />
                    {esCredito && (
                        <NumberInput
                            label="Plazo credito (dias)"
                            size="sm"
                            placeholder="30"
                            value={Number(values[VENTA_FIELDS.PLAZO_CREDITO_DIAS] ?? 0) || ""}
                            onChange={(value) =>
                                onFieldChange(
                                    VENTA_FIELDS.PLAZO_CREDITO_DIAS,
                                    value === ""
                                        ? null
                                        : typeof value === "number"
                                          ? value
                                          : parseInt(String(value), 10) || null,
                                )
                            }
                            error={errors[VENTA_FIELDS.PLAZO_CREDITO_DIAS]}
                            min={1}
                            allowDecimal={false}
                            disabled={disabled}
                        />
                    )}
                    <Stack gap={4}>
                        <Text size="sm" fw={600} c="dimmed">Moneda</Text>
                        <SegmentedControl
                            size="sm"
                            fullWidth
                            data={[...MONEDAS]}
                            value={monedaCodigo}
                            onChange={(value) => onFieldChange(VENTA_FIELDS.MONEDA_CODIGO, value.toUpperCase())}
                            disabled={disabled}
                        />
                        {errors[VENTA_FIELDS.MONEDA_CODIGO] && (
                            <Text size="xs" c="red">{errors[VENTA_FIELDS.MONEDA_CODIGO]}</Text>
                        )}
                    </Stack>
                    <NumberInput
                        label="Tipo de cambio"
                        size="sm"
                        description={
                            monedaCodigo === "CRC"
                                ? "Se usa si entra un pago en USD."
                                : "Se usa para expresar la factura en USD."
                        }
                        value={Number(values[VENTA_FIELDS.TIPO_CAMBIO] ?? 1)}
                        onChange={(value) =>
                            onFieldChange(
                                VENTA_FIELDS.TIPO_CAMBIO,
                                typeof value === "number" ? value : parseFloat(String(value)) || 1,
                            )
                        }
                        error={errors[VENTA_FIELDS.TIPO_CAMBIO]}
                        decimalScale={5}
                        min={0.00001}
                        disabled={disabled}
                    />
                </Stack>
            </Stack>
        </Card>
    );
}
