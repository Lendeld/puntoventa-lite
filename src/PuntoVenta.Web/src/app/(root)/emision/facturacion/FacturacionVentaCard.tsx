"use client";

import { VENTA_FIELDS } from "@lib/constants/ventas.constants";
import { ClienteFacturaSelect } from "@pages/emision/facturacion/ClienteFacturaSelect";
import type { FacturacionDocumentoCardProps } from "@pages/emision/facturacion/FacturacionDocumentoCard";
import { Card, Group, Select, Stack, Text, ThemeIcon } from "@mantine/core";
import { DateTimePicker } from "@mantine/dates";
import { IconFileInvoice } from "@tabler/icons-react";
import dayjs from "dayjs";
import "dayjs/locale/es";
import { useMemo } from "react";

export function FacturacionVentaCard({
    values,
    errors,
    disabled,
    clienteSeleccionado,
    aplicaVendedores,
    vendedores,
    aplicaCajas,
    cajas,
    onClienteChange,
    onClienteSeleccionadoChange,
    onFieldChange,
    showFechaVencimiento = false,
    fechaVencimientoLabel = "Vencimiento",
}: FacturacionDocumentoCardProps) {
    const maxFechaDocumento = useMemo(() => new Date(), []);
    const fechaDocumento = String(values[VENTA_FIELDS.FECHA_DOCUMENTO] ?? "");
    const fechaVencimiento = String(values[VENTA_FIELDS.FECHA_VENCIMIENTO] ?? "");
    const vendedorId = String(values[VENTA_FIELDS.VENDEDOR_ID] ?? "");
    const cajaId = String(values[VENTA_FIELDS.CAJA_ID] ?? "");

    return (
        <Card radius="lg" p="md" className="h-full">
            <Stack gap="sm">
                <Group gap="xs" align="center">
                    <ThemeIcon variant="light" color="accentPV" size="sm">
                        <IconFileInvoice size={15} />
                    </ThemeIcon>
                    <Text fw={700}>Venta</Text>
                </Group>
                <Stack gap="sm">
                    <DateTimePicker
                        label="Fecha del documento"
                        size="sm"
                        value={fechaDocumento ? dayjs(fechaDocumento).toDate() : null}
                        onChange={(value) =>
                            onFieldChange(
                                VENTA_FIELDS.FECHA_DOCUMENTO,
                                value ? dayjs(value).toISOString() : "",
                            )
                        }
                        error={errors[VENTA_FIELDS.FECHA_DOCUMENTO]}
                        disabled={disabled}
                        valueFormat="DD/MM/YYYY hh:mm A"
                        locale="es"
                        timePickerProps={{ format: "12h" }}
                        maxDate={maxFechaDocumento}
                        submitButtonProps={{ style: { display: "none" } }}
                        clearable={false}
                    />
                    {showFechaVencimiento && (
                        <DateTimePicker
                            label={fechaVencimientoLabel}
                            size="sm"
                            value={fechaVencimiento ? dayjs(fechaVencimiento).toDate() : null}
                            onChange={(value) =>
                                onFieldChange(
                                    VENTA_FIELDS.FECHA_VENCIMIENTO,
                                    value ? dayjs(value).toISOString() : "",
                                )
                            }
                            error={errors[VENTA_FIELDS.FECHA_VENCIMIENTO]}
                            disabled={disabled}
                            valueFormat="DD/MM/YYYY hh:mm A"
                            locale="es"
                            timePickerProps={{ format: "12h" }}
                            minDate={fechaDocumento ? dayjs(fechaDocumento).toDate() : undefined}
                            submitButtonProps={{ style: { display: "none" } }}
                            clearable={false}
                        />
                    )}
                    {aplicaVendedores && (
                        <Select
                            label="Vendedor"
                            size="sm"
                            placeholder="Selecciona un vendedor"
                            data={vendedores.map((vendedor) => ({
                                value: vendedor.id,
                                label: vendedor.isPrincipal
                                    ? `${vendedor.nombre} · Principal`
                                    : vendedor.nombre,
                            }))}
                            value={vendedorId || null}
                            onChange={(value) => onFieldChange(VENTA_FIELDS.VENDEDOR_ID, value ?? "")}
                            error={errors[VENTA_FIELDS.VENDEDOR_ID]}
                            disabled={disabled || vendedores.length === 0}
                            clearable
                        />
                    )}
                    {aplicaCajas && (
                        <Select
                            label="Caja"
                            size="sm"
                            placeholder="Selecciona una caja"
                            data={cajas.map((caja) => ({
                                value: caja.id,
                                label: caja.nombre,
                            }))}
                            value={cajaId || null}
                            onChange={(value) => onFieldChange(VENTA_FIELDS.CAJA_ID, value ?? "")}
                            error={errors[VENTA_FIELDS.CAJA_ID]}
                            disabled={disabled || cajas.length === 0}
                            clearable
                        />
                    )}
                    <Stack gap={4}>
                        <ClienteFacturaSelect
                            label="Cliente"
                            placeholder="Buscar cliente por nombre o identificación"
                            size="sm"
                            value={String(values[VENTA_FIELDS.CLIENTE_ID] ?? "")}
                            onChange={(value) => onClienteChange(value ?? "")}
                            onClienteChange={onClienteSeleccionadoChange}
                            error={errors[VENTA_FIELDS.CLIENTE_ID] as string | undefined}
                            disabled={disabled}
                        />
                        <Text size="xs" c="dimmed" lineClamp={1}>
                            {clienteSeleccionado
                                ? clienteSeleccionado.correo ||
                                  clienteSeleccionado.telefono ||
                                  "Cliente seleccionado."
                                : "La factura puede emitirse sin cliente cuando aplique."}
                        </Text>
                    </Stack>
                </Stack>
            </Stack>
        </Card>
    );
}
