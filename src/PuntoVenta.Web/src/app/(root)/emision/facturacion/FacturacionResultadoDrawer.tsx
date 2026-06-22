"use client";

import { useImprimirTicketAhora } from "@lib/printing/imprimir-ticket";
import { getVentaPdfUrl } from "@lib/printing/venta-printing";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { Button, Drawer, Group, Stack, Text } from "@mantine/core";
import { IconFileTypePdf, IconPrinter } from "@tabler/icons-react";

interface Props {
    opened: boolean;
    documento: DocumentoVentaDto | null;
    vuelto: number;
    onClose: () => void;
}

export function FacturacionResultadoDrawer({
    opened,
    documento,
    vuelto,
    onClose,
}: Props) {
    const imprimirTicket = useImprimirTicketAhora();
    const monedaCodigo = documento?.monedaCodigo ?? "CRC";
    const consecutivo = documento?.consecutivo ?? "Documento sin consecutivo";

    return (
        <Drawer
            opened={opened}
            onClose={onClose}
            position="bottom"
            size="50dvh"
            padding="xl"
            radius="lg"
            title={
                <Text fw={800} size="1.35rem">
                    Factura emitida
                </Text>
            }
            overlayProps={{ backgroundOpacity: 0.35, blur: 2 }}
            classNames={{
                content: "bg-theme-surface border border-theme",
                header: "bg-theme-surface",
            }}
            closeButtonProps={{ "aria-label": "Cerrar confirmacion" }}
        >
            <Group justify="space-between" align="center" gap="xl" w={"100%"}>
                <Stack gap={4}>
                    <Text size="sm" c="dimmed">
                        Numero de factura
                    </Text>
                    <Text fw={800} size="1.45rem">
                        {consecutivo}
                    </Text>
                </Stack>

                <Stack gap={4}>
                    <Text size="sm" c="dimmed">
                        Vuelto
                    </Text>
                    <Text fw={900} size="2.4rem" c="teal">
                        {formatMonedaPorCodigo(vuelto, monedaCodigo)}
                    </Text>
                </Stack>
            </Group>
            <Group gap="xs" justify="center" w="100%" mt={68}>
                <Button
                    leftSection={<IconPrinter size={18} />}
                    onClick={() => documento && imprimirTicket(documento.id)}
                    disabled={!documento}
                    size="md"
                >
                    Imprimir ticket
                </Button>
                <Button
                    component="a"
                    href={documento ? getVentaPdfUrl(documento.id) : "#"}
                    target="_blank"
                    rel="noopener noreferrer"
                    variant="light"
                    leftSection={<IconFileTypePdf size={18} />}
                    disabled={!documento}
                    size="md"
                >
                    Ver PDF
                </Button>
            </Group>
        </Drawer>
    );
}
