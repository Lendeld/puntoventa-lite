"use client";

import type { ProductoDto } from "@lib/types/productos.types";
import type { DocumentoVentaLineaForm } from "@lib/types/ventas.types";
import { ProductoFacturaSelect } from "@pages/emision/facturacion/ProductoFacturaSelect";
import {
    FacturacionLineaMobileCard,
    FacturacionLineaTableRow,
    LineasVacias,
    type FacturacionLineaEditorProps,
} from "@pages/emision/facturacion/FacturacionLineaEditor";
import { Box, Card, Group, ScrollArea, Stack, Table, Text } from "@mantine/core";

interface Props {
    lineas: DocumentoVentaLineaForm[];
    monedaCodigo: string;
    disabled: boolean;
    lineasError?: string | null;
    selectedProductoId: string | null;
    onSelectedProductoIdChange: (value: string | null) => void;
    onSelectedProductoChange: (producto: ProductoDto | null) => void;
    onProductoSeleccionado: (producto: ProductoDto) => void;
    onUpdateLinea: FacturacionLineaEditorProps["onUpdateLinea"];
    onRemoveLinea: FacturacionLineaEditorProps["onRemoveLinea"];
    getFieldError: FacturacionLineaEditorProps["getFieldError"];
}

const LINEAS_TABLE_MAX_HEIGHT = 440;

export function FacturacionLineasEditor({
    lineas,
    monedaCodigo,
    disabled,
    lineasError,
    selectedProductoId,
    onSelectedProductoIdChange,
    onSelectedProductoChange,
    onProductoSeleccionado,
    onUpdateLinea,
    onRemoveLinea,
    getFieldError,
}: Props) {
    const editorProps = {
        monedaCodigo,
        disabled,
        onUpdateLinea,
        onRemoveLinea,
        getFieldError,
    };

    return (
        <Card radius="lg" p="lg">
            <Stack gap="md">
                <Group justify="space-between" align="flex-start" gap="md">
                    <Stack gap={2}>
                        <Text fw={700}>Líneas de factura</Text>
                        <Text size="sm" c="dimmed">
                            Agrega bienes o servicios y ajusta cantidad y descuento.
                        </Text>
                    </Stack>
                </Group>

                <ProductoFacturaSelect
                    label="Buscar producto o servicio"
                    placeholder="Escribe nombre o código"
                    value={selectedProductoId}
                    onChange={onSelectedProductoIdChange}
                    onProductoChange={onSelectedProductoChange}
                    onProductoSeleccionado={onProductoSeleccionado}
                    monedaCodigo={monedaCodigo}
                    disabled={disabled}
                    className="w-full"
                />

                {lineasError && <Text c="red" size="sm">{lineasError}</Text>}

                <Box visibleFrom="md">
                    <ScrollArea.Autosize
                        mah={LINEAS_TABLE_MAX_HEIGHT}
                        offsetScrollbars
                        scrollbarSize={8}
                        type="auto"
                    >
                        <Table striped highlightOnHover stickyHeader stickyHeaderOffset={0} miw={980}>
                            <Table.Thead>
                                <Table.Tr>
                                    <Table.Th>Producto</Table.Th>
                                    <Table.Th ta="right" w={110}>Cantidad</Table.Th>
                                    <Table.Th ta="right" w={150}>Precio</Table.Th>
                                    <Table.Th ta="right" w={150}>Descuento</Table.Th>
                                    <Table.Th ta="right" w={150}>Total</Table.Th>
                                    <Table.Th w={60} />
                                </Table.Tr>
                            </Table.Thead>
                            <Table.Tbody>
                                {lineas.length === 0 ? (
                                    <Table.Tr>
                                        <Table.Td colSpan={6}><LineasVacias /></Table.Td>
                                    </Table.Tr>
                                ) : (
                                    lineas.map((linea, index) => (
                                        <FacturacionLineaTableRow
                                            key={linea.Id ?? linea.ProductoId}
                                            linea={linea}
                                            index={index}
                                            {...editorProps}
                                        />
                                    ))
                                )}
                            </Table.Tbody>
                        </Table>
                    </ScrollArea.Autosize>
                </Box>

                <Box hiddenFrom="md">
                    {lineas.length === 0 ? (
                        <LineasVacias />
                    ) : (
                        <Stack gap="sm">
                            {lineas.map((linea, index) => (
                                <FacturacionLineaMobileCard
                                    key={linea.Id ?? linea.ProductoId}
                                    linea={linea}
                                    index={index}
                                    {...editorProps}
                                />
                            ))}
                        </Stack>
                    )}
                </Box>
            </Stack>
        </Card>
    );
}
