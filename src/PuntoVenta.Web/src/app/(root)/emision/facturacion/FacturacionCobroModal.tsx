"use client";

import type { DocumentoVentaPagoForm } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { Badge, Button, Group, Modal, Stack, Text } from "@mantine/core";
import { IconCheck } from "@tabler/icons-react";
import { FacturacionPagoRapido } from "@pages/emision/facturacion/FacturacionPagoRapido";
import { FacturacionPagosEditor } from "@pages/emision/facturacion/FacturacionPagosEditor";

interface Props {
    opened: boolean;
    modo: "rapido" | "detallado";
    total: number;
    pagado: number;
    saldo: number;
    monedaCodigo: string;
    equivalenteTotal?: number | null;
    monedaEquivalente?: string;
    pagos: DocumentoVentaPagoForm[];
    pagosError?: string | null;
    loading: boolean;
    onClose: () => void;
    onAgregarPago: () => void;
    onUpdatePago: (
        index: number,
        field: keyof DocumentoVentaPagoForm,
        value: unknown,
    ) => void;
    onRemovePago: (index: number) => void;
    getFieldError: (path: string) => string | null;
    onConfirm: () => void;
}

export function FacturacionCobroModal({
    opened,
    modo,
    total,
    pagado,
    saldo,
    monedaCodigo,
    equivalenteTotal,
    monedaEquivalente,
    pagos,
    pagosError,
    loading,
    onClose,
    onAgregarPago,
    onUpdatePago,
    onRemovePago,
    getFieldError,
    onConfirm,
}: Props) {
    return (
        <Modal
            opened={opened}
            onClose={onClose}
            centered
            withCloseButton
            size="xl"
            radius={28}
            padding="xl"
            title={
                <Text fw={800} className="text-2xl sm:text-[2rem]">
                    {modo === "rapido" ? "Cobro rapido" : "Cobro detallado"}
                </Text>
            }
            overlayProps={{ backgroundOpacity: 0.55, blur: 3 }}
            classNames={{
                content: "bg-theme-surface border border-theme",
                header: "bg-theme-surface",
                body: "pt-5",
            }}
            closeOnClickOutside={false}
        >
            <Stack className="gap-4 sm:gap-6">
                <Group justify="space-between" align="flex-start" gap="md" wrap="wrap">
                    <Stack gap={2}>
                        <Text size="sm" c="dimmed">
                            Total factura
                        </Text>
                        <Text fw={800} className="text-xl sm:text-[1.6rem] leading-none">
                            {formatMonedaPorCodigo(total, monedaCodigo)}
                        </Text>
                        {equivalenteTotal != null && monedaEquivalente && (
                            <Badge
                                variant="light"
                                color="teal"
                                size="lg"
                                radius="sm"
                                className="self-start normal-case mt-1.5"
                            >
                                Equiv.{" "}
                                {formatMonedaPorCodigo(
                                    equivalenteTotal,
                                    monedaEquivalente,
                                )}
                            </Badge>
                        )}
                    </Stack>
                    <Stack gap={2}>
                        <Text size="sm" c="dimmed">
                            Total aplicado
                        </Text>
                        <Text fw={800} className="text-xl sm:text-[1.6rem] leading-none">
                            {formatMonedaPorCodigo(pagado, monedaCodigo)}
                        </Text>
                    </Stack>
                    <Stack gap={2}>
                        <Text size="sm" c="dimmed">
                            Saldo restante
                        </Text>
                        <Text
                            fw={800}
                            c={saldo === 0 ? "teal" : "orange"}
                            className="text-xl sm:text-[1.6rem] leading-none"
                        >
                            {formatMonedaPorCodigo(saldo, monedaCodigo)}
                        </Text>
                    </Stack>
                </Group>

                {modo === "rapido" && pagos[0] ? (
                    <FacturacionPagoRapido
                        pago={pagos[0]}
                        monedaDocumento={monedaCodigo}
                        total={total}
                        disabled={loading}
                        getFieldError={getFieldError}
                        onUpdatePago={(field, value) =>
                            onUpdatePago(0, field, value)
                        }
                    />
                ) : (
                    <FacturacionPagosEditor
                        pagos={pagos}
                        monedaDocumento={monedaCodigo}
                        disabled={loading}
                        pagosError={pagosError}
                        onAgregarPago={onAgregarPago}
                        onUpdatePago={onUpdatePago}
                        onRemovePago={onRemovePago}
                        getFieldError={getFieldError}
                    />
                )}

                <Button
                    size="xl"
                    radius="xl"
                    fullWidth
                    leftSection={<IconCheck size={22} />}
                    onClick={onConfirm}
                    loading={loading}
                    disabled={pagos.length === 0 || saldo !== 0}
                    data-autofocus
                    className="h-14 text-base font-extrabold sm:h-17 sm:text-[1.1rem]"
                >
                    Confirmar cobro
                </Button>
            </Stack>
        </Modal>
    );
}
