"use client";

import type { DocumentoVentaLineaForm } from "@lib/types/ventas.types";
import { colorPorTipoItem } from "@lib/utils/productos.utils";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    ActionIcon,
    Badge,
    Card,
    Divider,
    Group,
    NumberInput,
    Popover,
    Stack,
    Table,
    Text,
    Textarea,
    ThemeIcon,
} from "@mantine/core";
import { IconInfoCircle, IconShoppingCartPlus, IconTrash } from "@tabler/icons-react";

export interface FacturacionLineaEditorProps {
    linea: DocumentoVentaLineaForm;
    index: number;
    monedaCodigo: string;
    disabled: boolean;
    onUpdateLinea: (
        index: number,
        field: keyof DocumentoVentaLineaForm,
        value: unknown,
    ) => void;
    onRemoveLinea: (index: number) => void;
    getFieldError: (path: string) => string | null;
}

interface LineaTotales {
    montoBruto: number;
    total: number;
}

function calcularLineaTotales(linea: DocumentoVentaLineaForm): LineaTotales {
    const montoBruto = linea.Cantidad * linea.PrecioUnitario;
    const subtotal = Math.max(montoBruto - linea.MontoDescuento, 0);
    return {
        montoBruto,
        total: subtotal * (1 + (linea.PorcentajeImpuesto ?? 0) / 100),
    };
}

function parseNumero(value: string | number) {
    return typeof value === "number" ? value : parseFloat(String(value)) || 0;
}

export function LineasVacias() {
    return (
        <Stack align="center" gap={6} py="xl">
            <ThemeIcon variant="light" radius="xl" size={48} color="gray">
                <IconShoppingCartPlus size={24} />
            </ThemeIcon>
            <Text fw={600} size="sm">Aún no hay líneas</Text>
            <Text size="xs" c="dimmed" ta="center" maw={280}>
                Busca un producto arriba o escanea un código para empezar.
            </Text>
        </Stack>
    );
}

function DescripcionPopover({
    linea,
    index,
    disabled,
    onUpdateLinea,
    mobile = false,
}: Pick<FacturacionLineaEditorProps, "linea" | "index" | "disabled" | "onUpdateLinea"> & {
    mobile?: boolean;
}) {
    return (
        <Popover withinPortal width={mobile ? 260 : 280} position="bottom-start" shadow="md">
            <Popover.Target>
                <Stack gap={2} style={{ cursor: "pointer", flex: 1, minWidth: 0 }}>
                    <Group gap="xs" wrap="nowrap">
                        <Text fw={600} size="sm" truncate={mobile || undefined}>
                            {mobile ? linea.Descripcion : linea.Codigo}
                        </Text>
                        <IconInfoCircle size={mobile ? 13 : 14} />
                    </Group>
                    {mobile ? (
                        <Text size="xs" c="dimmed" ff="monospace">
                            {linea.Codigo}
                        </Text>
                    ) : (
                        <>
                            {linea.Descripcion && <Text size="xs" c="dimmed" lineClamp={2}>{linea.Descripcion}</Text>}
                            <Text size="xs" c="dimmed">
                                IVA {linea.PorcentajeImpuesto ?? 0}%
                            </Text>
                        </>
                    )}
                </Stack>
            </Popover.Target>
            <Popover.Dropdown>
                <Stack gap="xs">
                    <Group gap="xs">
                        <Badge size="xs" variant="light" color={colorPorTipoItem(linea.TipoItem)}>
                            {linea.TipoItem === "Bien" ? "Bien" : "Servicio"}
                        </Badge>
                        <Text size="xs" c="dimmed">IVA: {linea.PorcentajeImpuesto}%</Text>
                    </Group>
                    <Textarea
                        size="xs"
                        label="Descripción en factura"
                        placeholder="Nombre personalizado para el cliente"
                        autosize
                        minRows={2}
                        value={linea.Descripcion}
                        onChange={(event) => onUpdateLinea(index, "Descripcion", event.currentTarget.value)}
                        disabled={disabled}
                    />
                </Stack>
            </Popover.Dropdown>
        </Popover>
    );
}

function LineaNumeroInput({
    linea,
    index,
    field,
    disabled,
    getFieldError,
    onUpdateLinea,
    label,
    max,
}: Pick<FacturacionLineaEditorProps, "linea" | "index" | "disabled" | "getFieldError" | "onUpdateLinea"> & {
    field: "Cantidad" | "PrecioUnitario" | "MontoDescuento";
    label?: string;
    max?: number;
}) {
    return (
        <NumberInput
            label={label}
            size={label ? "xs" : undefined}
            value={linea[field]}
            onChange={(value) => onUpdateLinea(index, field, parseNumero(value))}
            min={field === "MontoDescuento" ? 0 : 0.00001}
            max={max}
            decimalScale={5}
            styles={{ input: { textAlign: "right" } }}
            error={getFieldError(`DocumentoVenta_Lineas.${index}.${field}`) ?? undefined}
            disabled={disabled}
        />
    );
}

function PrecioLinea({
    linea,
    index,
    monedaCodigo,
    disabled,
    getFieldError,
    onUpdateLinea,
    mobile = false,
}: Omit<FacturacionLineaEditorProps, "onRemoveLinea"> & { mobile?: boolean }) {
    if (linea.PermiteModificarPrecioUnitario) {
        return (
            <LineaNumeroInput
                linea={linea}
                index={index}
                field="PrecioUnitario"
                label={mobile ? "Precio" : undefined}
                disabled={disabled}
                getFieldError={getFieldError}
                onUpdateLinea={onUpdateLinea}
            />
        );
    }

    return (
        <Stack gap={2}>
            {mobile && <Text size="xs" c="dimmed" fw={500}>Precio</Text>}
            <Text fw={600} size="sm" ta="right">
                {formatMonedaPorCodigo(linea.PrecioUnitario, monedaCodigo)}
            </Text>
        </Stack>
    );
}

export function FacturacionLineaTableRow(props: FacturacionLineaEditorProps) {
    const { linea, index, monedaCodigo, disabled, onRemoveLinea } = props;
    const { montoBruto, total } = calcularLineaTotales(linea);

    return (
        <Table.Tr>
            <Table.Td><DescripcionPopover {...props} /></Table.Td>
            <Table.Td><LineaNumeroInput {...props} field="Cantidad" /></Table.Td>
            <Table.Td><PrecioLinea {...props} /></Table.Td>
            <Table.Td><LineaNumeroInput {...props} field="MontoDescuento" max={montoBruto} /></Table.Td>
            <Table.Td ta="right">
                <Text fw={700} size="sm" className="tabular-nums">
                    {formatMonedaPorCodigo(total, monedaCodigo)}
                </Text>
            </Table.Td>
            <Table.Td ta="right">
                <ActionIcon
                    variant="light"
                    color="red"
                    onClick={() => onRemoveLinea(index)}
                    disabled={disabled}
                    aria-label={`Eliminar línea ${index + 1}`}
                >
                    <IconTrash size={16} />
                </ActionIcon>
            </Table.Td>
        </Table.Tr>
    );
}

export function FacturacionLineaMobileCard(props: FacturacionLineaEditorProps) {
    const { linea, index, monedaCodigo, disabled, onRemoveLinea } = props;
    const { montoBruto, total } = calcularLineaTotales(linea);

    return (
        <Card radius="md" p="sm" withBorder>
            <Stack gap="xs">
                <Group justify="space-between" align="flex-start" wrap="nowrap">
                    <DescripcionPopover {...props} mobile />
                    <ActionIcon
                        variant="light"
                        color="red"
                        size="sm"
                        onClick={() => onRemoveLinea(index)}
                        disabled={disabled}
                        aria-label={`Eliminar línea ${index + 1}`}
                    >
                        <IconTrash size={14} />
                    </ActionIcon>
                </Group>
                <Divider />
                <Group grow gap="xs">
                    <LineaNumeroInput {...props} field="Cantidad" label="Cantidad" />
                    <PrecioLinea {...props} mobile />
                    <LineaNumeroInput {...props} field="MontoDescuento" label="Descuento" max={montoBruto} />
                </Group>
                <Divider />
                <Group justify="space-between">
                    <Text size="xs" c="dimmed">IVA: {linea.PorcentajeImpuesto}%</Text>
                    <Text fw={700} size="sm" className="tabular-nums">
                        {formatMonedaPorCodigo(total, monedaCodigo)}
                    </Text>
                </Group>
            </Stack>
        </Card>
    );
}
