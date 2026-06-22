"use client";

import { Alert, Badge, Group, Stack, Text } from "@mantine/core";
import { IconAlertTriangle, IconCreditCard, IconExclamationCircle } from "@tabler/icons-react";
import { esCondicionVentaCredito } from "@lib/constants/ventas.constants";
import { useSaldoCreditoClienteQuery } from "@lib/hooks/useCreditoQuery";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";

interface Props {
    clienteId: string | null | undefined;
    condicionVentaCodigo: string;
    plazoActual: number | null;
    onSugerirPlazo?: (dias: number) => void;
}

export function FacturacionCreditoBanner({
    clienteId,
    condicionVentaCodigo,
}: Props) {
    const esCredito = esCondicionVentaCredito(condicionVentaCodigo);
    const habilitado = esCredito && !!clienteId;

    const { data, isLoading } = useSaldoCreditoClienteQuery(habilitado ? clienteId : null);

    if (!esCredito) return null;

    if (!clienteId) {
        return (
            <Alert
                color="yellow"
                variant="light"
                icon={<IconAlertTriangle size={16} />}
                title="Cliente requerido"
            >
                Las ventas a crédito requieren seleccionar un cliente identificado.
            </Alert>
        );
    }

    if (isLoading || !data) {
        return (
            <Alert color="blue" variant="light" icon={<IconCreditCard size={16} />}>
                Consultando estado de crédito…
            </Alert>
        );
    }

    if (data.esMoroso) {
        return (
            <Alert
                color="red"
                variant="light"
                icon={<IconExclamationCircle size={16} />}
                title="Cliente con facturas vencidas"
            >
                <Stack gap={4}>
                    <Text size="sm">
                        {data.facturasVencidas} factura{data.facturasVencidas === 1 ? "" : "s"} vencida{data.facturasVencidas === 1 ? "" : "s"}
                        {" "}· máximo {data.diasAtrasoMax} día{data.diasAtrasoMax === 1 ? "" : "s"} de atraso.
                    </Text>
                    <Text size="sm">
                        Saldo vencido: {formatMonedaPorCodigo(data.saldoVencido, "CRC")}
                    </Text>
                </Stack>
            </Alert>
        );
    }

    return (
        <Alert color="green" variant="light" icon={<IconCreditCard size={16} />}>
            <Group justify="space-between" wrap="wrap">
                <Stack gap={2}>
                    <Text size="sm" fw={600}>
                        Saldo vigente: {formatMonedaPorCodigo(data.saldoVigente, "CRC")}
                    </Text>
                    <Text size="xs" c="dimmed">
                        {data.facturasVencidas === 0 ? "Sin facturas vencidas" : `${data.facturasVencidas} facturas vencidas`}
                    </Text>
                </Stack>
                <Badge color="green" variant="light">Al día</Badge>
            </Group>
        </Alert>
    );
}
