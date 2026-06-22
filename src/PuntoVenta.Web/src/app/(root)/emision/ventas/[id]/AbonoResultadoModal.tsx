"use client";

import { useImprimirTicketAhora } from "@lib/printing/imprimir-ticket";
import { getAbonoPdfUrl } from "@lib/printing/venta-printing";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { Button, Group, Modal, Stack, Text } from "@mantine/core";
import { IconFileTypePdf, IconPrinter } from "@tabler/icons-react";

interface Props {
    opened: boolean;
    facturaId: string;
    pagoId: string;
    consecutivo?: string | null;
    montoAbono?: number | null;
    monedaCodigo?: string | null;
    onClose: () => void;
}

export function AbonoResultadoModal({
    opened,
    facturaId,
    pagoId,
    consecutivo,
    montoAbono,
    monedaCodigo,
    onClose,
}: Props) {
    const imprimirTicket = useImprimirTicketAhora();

    return (
        <Modal
            opened={opened}
            onClose={onClose}
            title="Abono registrado"
            centered
            size="sm"
        >
            <Stack gap="lg">
                <Stack gap={4}>
                    {consecutivo && (
                        <Group justify="space-between">
                            <Text size="sm" c="dimmed">
                                Factura
                            </Text>
                            <Text fw={700}>{consecutivo}</Text>
                        </Group>
                    )}
                    {montoAbono != null && (
                        <Group justify="space-between">
                            <Text size="sm" c="dimmed">
                                Monto abonado
                            </Text>
                            <Text fw={700} c="teal">
                                {formatMonedaPorCodigo(
                                    montoAbono,
                                    monedaCodigo ?? "CRC",
                                )}
                            </Text>
                        </Group>
                    )}
                </Stack>

                <Group gap="xs" grow>
                    <Button
                        leftSection={<IconPrinter size={16} />}
                        onClick={() => imprimirTicket(facturaId, pagoId)}
                    >
                        Imprimir ticket
                    </Button>
                    <Button
                        component="a"
                        href={getAbonoPdfUrl(facturaId, pagoId)}
                        target="_blank"
                        rel="noopener noreferrer"
                        variant="light"
                        leftSection={<IconFileTypePdf size={16} />}
                    >
                        Ver PDF
                    </Button>
                </Group>

                <Group justify="flex-end">
                    <Button variant="light" onClick={onClose}>
                        Cerrar
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
