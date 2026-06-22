"use client";

import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo, getEstadoDocumentoLabel } from "@lib/utils/ventas.utils";
import { Badge, Box, Button, Group, Menu, Paper, Stack, Text } from "@mantine/core";
import {
    IconCheck,
    IconChevronDown,
    IconDeviceFloppy,
    IconReceipt2,
    IconStack2,
} from "@tabler/icons-react";

interface Totales {
    subtotal: number;
    descuentos: number;
    impuesto: number;
    total: number;
}

interface FooterEstado {
    puedeFacturar: boolean;
    canEmit: boolean;
    loadingEmitir: boolean;
    loadingGuardar: boolean;
    showOpcionesCobro: boolean;
    disableCrearApartado: boolean;
    disableGuardarProforma: boolean;
}

interface Props {
    documento: DocumentoVentaDto | null;
    totales: Totales;
    monedaCodigo: string;
    monedaEquivalente: string;
    equivalenteTotal: number | null;
    emitirLabel: string;
    estado: FooterEstado;
    onNuevaFactura: () => void;
    onCrearApartado: () => void;
    onGuardarProforma: () => void;
    onEmitir: () => void;
    onCobroDetallado: () => void;
}

export function FacturacionFooterBar({
    documento,
    totales,
    monedaCodigo,
    monedaEquivalente,
    equivalenteTotal,
    emitirLabel,
    estado,
    onNuevaFactura,
    onCrearApartado,
    onGuardarProforma,
    onEmitir,
    onCobroDetallado,
}: Props) {
    const {
        puedeFacturar,
        canEmit,
        loadingEmitir,
        loadingGuardar,
        showOpcionesCobro,
        disableCrearApartado,
        disableGuardarProforma,
    } = estado;

    return (
        <Paper
            radius={0}
            p="sm"
            style={{
                left: "var(--app-shell-navbar-offset, 0rem)",
                right: "var(--app-shell-aside-offset, 0rem)",
                transition: "left 200ms ease, right 200ms ease",
            }}
            className="fixed bottom-0 z-40 border-t border-theme bg-theme-surface shadow-lg"
        >
            <Group justify="space-between" gap="md" wrap="wrap">
                <Group gap="xs" wrap="wrap">
                    <Badge color={documento ? "blue" : "gray"} variant="light">
                        {getEstadoDocumentoLabel(documento?.estado)}
                    </Badge>
                    {documento?.consecutivo && (
                        <Badge
                            color="blue"
                            variant="outline"
                            leftSection={<IconReceipt2 size={12} />}
                        >
                            {documento.consecutivo}
                        </Badge>
                    )}
                    <Button variant="light" size="xs" onClick={onNuevaFactura}>
                        Nueva factura
                    </Button>
                    <Button
                        variant="light"
                        color="orange"
                        size="xs"
                        onClick={onCrearApartado}
                        loading={loadingGuardar}
                        disabled={disableCrearApartado}
                    >
                        Crear apartado
                    </Button>
                    <Button
                        variant="light"
                        size="xs"
                        leftSection={<IconDeviceFloppy size={14} />}
                        onClick={onGuardarProforma}
                        loading={loadingGuardar}
                        disabled={disableGuardarProforma}
                    >
                        Guardar proforma
                    </Button>
                </Group>

                <Group gap="md" justify="flex-end" wrap="wrap">
                    <Box visibleFrom="sm">
                        <Stack gap={0} align="flex-end">
                            <Text size="xs" c="dimmed">
                                Subtotal
                            </Text>
                            <Text size="sm" fw={600}>
                                {formatMonedaPorCodigo(totales.subtotal, monedaCodigo)}
                            </Text>
                        </Stack>
                    </Box>
                    <Box visibleFrom="md">
                        <Stack gap={0} align="flex-end">
                            <Text size="xs" c="dimmed">
                                Descuento
                            </Text>
                            <Text size="sm" fw={600}>
                                {formatMonedaPorCodigo(totales.descuentos, monedaCodigo)}
                            </Text>
                        </Stack>
                    </Box>
                    <Box visibleFrom="sm">
                        <Stack gap={0} align="flex-end">
                            <Text size="xs" c="dimmed">
                                IVA
                            </Text>
                            <Text size="sm" fw={600}>
                                {formatMonedaPorCodigo(totales.impuesto, monedaCodigo)}
                            </Text>
                        </Stack>
                    </Box>
                    {equivalenteTotal != null && (
                        <Box visibleFrom="lg">
                            <Stack gap={0} align="flex-end">
                                <Text size="xs" c="dimmed">
                                    Equiv. {monedaEquivalente}
                                </Text>
                                <Text size="sm" fw={600}>
                                    {formatMonedaPorCodigo(
                                        equivalenteTotal,
                                        monedaEquivalente,
                                    )}
                                </Text>
                            </Stack>
                        </Box>
                    )}
                    <Stack gap={0} align="flex-end">
                        <Text size="xs" c="dimmed" fw={600}>
                            Total
                        </Text>
                        <Text
                            size="xl"
                            fw={800}
                            className="tabular-nums text-theme-accent"
                        >
                            {formatMonedaPorCodigo(totales.total, monedaCodigo)}
                        </Text>
                    </Stack>
                    {showOpcionesCobro ? (
                        <Group gap={0} wrap="nowrap">
                            <Button
                                color="green"
                                leftSection={<IconCheck size={16} />}
                                onClick={onEmitir}
                                loading={loadingEmitir}
                                disabled={!puedeFacturar || !canEmit}
                                styles={{
                                    root: {
                                        borderTopRightRadius: 0,
                                        borderBottomRightRadius: 0,
                                    },
                                }}
                            >
                                {emitirLabel}
                            </Button>
                            <Menu shadow="md" width={220} position="top-end">
                                <Menu.Target>
                                    <Button
                                        color="green"
                                        disabled={
                                            !puedeFacturar || !canEmit || loadingEmitir
                                        }
                                        px={8}
                                        styles={{
                                            root: {
                                                borderTopLeftRadius: 0,
                                                borderBottomLeftRadius: 0,
                                                borderLeft:
                                                    "1px solid rgba(255, 255, 255, 0.18)",
                                                minWidth: 34,
                                            },
                                        }}
                                    >
                                        <IconChevronDown size={16} />
                                    </Button>
                                </Menu.Target>
                                <Menu.Dropdown>
                                    <Menu.Item
                                        leftSection={<IconStack2 size={16} />}
                                        onClick={onCobroDetallado}
                                    >
                                        Cobro detallado
                                    </Menu.Item>
                                </Menu.Dropdown>
                            </Menu>
                        </Group>
                    ) : (
                        <Button
                            color="green"
                            leftSection={<IconCheck size={16} />}
                            onClick={onEmitir}
                            loading={loadingEmitir}
                            disabled={!puedeFacturar || !canEmit}
                        >
                            {emitirLabel}
                        </Button>
                    )}
                </Group>
            </Group>
        </Paper>
    );
}
