"use client";

import { anularAbonoFacturaAction } from "@lib/actions/ventas.actions";
import { AppNotifier } from "@components/ui/AppNotifier";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { AnulacionResultadoModal } from "@pages/emision/ventas/[id]/AnulacionResultadoModal";
import {
    Alert,
    Button,
    Group,
    Modal,
    Stack,
    Text,
    TextInput,
    Textarea,
} from "@mantine/core";
import { IconAlertTriangle } from "@tabler/icons-react";
import { useRouter } from "next/navigation";
import { useState } from "react";

const CONFIRMACION_REQUERIDA = "Anular Abono";

interface Props {
    documentoId: string;
    pagoId: string;
    montoAplicado: number;
    monedaCodigo: string;
    consecutivo?: string | null;
    onClose: () => void;
}

export function AnularAbonoModal({
    documentoId,
    pagoId,
    montoAplicado,
    monedaCodigo,
    consecutivo,
    onClose,
}: Props) {
    const router = useRouter();
    const [confirmacion, setConfirmacion] = useState("");
    const [motivo, setMotivo] = useState("");
    const [motivoError, setMotivoError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [resultadoVisible, setResultadoVisible] = useState(false);

    const confirmacionValida = confirmacion.trim() === CONFIRMACION_REQUERIDA;

    async function handleConfirmar() {
        setError(null);
        setMotivoError(null);

        const motivoNormalizado = motivo.trim();
        if (!motivoNormalizado || motivoNormalizado.length < 3) {
            setMotivoError("El motivo debe tener al menos 3 caracteres.");
            return;
        }

        if (motivoNormalizado.length > 255) {
            setMotivoError("El motivo no puede exceder 255 caracteres.");
            return;
        }

        if (!confirmacionValida) return;

        setLoading(true);
        const result = await anularAbonoFacturaAction(
            documentoId,
            pagoId,
            motivoNormalizado,
        );
        setLoading(false);

        if (result.errors) {
            const message = resolveApiErrorMessage(result.errors, {
                fallback: "No fue posible anular el abono.",
            });

            if ((result.status ?? 500) < 500) {
                setError(message);
            } else {
                AppNotifier.error({ message });
                onClose();
            }
            return;
        }

        setResultadoVisible(true);
    }

    function handleCerrarResultado() {
        setResultadoVisible(false);
        onClose();
        router.refresh();
    }

    if (resultadoVisible) {
        return (
            <AnulacionResultadoModal
                opened
                facturaId={documentoId}
                pagoId={pagoId}
                consecutivo={consecutivo}
                montoAplicado={montoAplicado}
                monedaCodigo={monedaCodigo}
                onClose={handleCerrarResultado}
            />
        );
    }

    return (
        <Modal
            opened
            onClose={onClose}
            title="Anular abono"
            centered
            closeOnClickOutside={!loading}
            closeOnEscape={!loading}
        >
            <Stack gap="md">
                <Alert
                    color="red"
                    variant="light"
                    icon={<IconAlertTriangle size={18} />}
                    title="Acción no reversible"
                >
                    Se revertirá el abono de{" "}
                    <strong>{formatMonedaPorCodigo(montoAplicado, monedaCodigo)}</strong>. El
                    saldo pendiente de la factura aumentará por ese monto y se conservará la
                    evidencia de anulación.
                </Alert>

                {error && (
                    <Alert color="orange" variant="light">
                        {error}
                    </Alert>
                )}

                <Textarea
                    label="Motivo de la anulación"
                    placeholder="Indica el motivo por el que se anula este abono"
                    value={motivo}
                    onChange={(event) => {
                        setMotivo(event.currentTarget.value);
                        setMotivoError(null);
                    }}
                    error={motivoError}
                    minLength={3}
                    maxLength={255}
                    rows={2}
                    required
                />

                <TextInput
                    label={
                        <Text size="sm">
                            Escribe{" "}
                            <Text component="span" fw={700} c="red">
                                {CONFIRMACION_REQUERIDA}
                            </Text>{" "}
                            para confirmar
                        </Text>
                    }
                    placeholder={CONFIRMACION_REQUERIDA}
                    value={confirmacion}
                    onChange={(event) => setConfirmacion(event.currentTarget.value)}
                    error={
                        confirmacion.length > 0 && !confirmacionValida
                            ? "El texto no coincide"
                            : undefined
                    }
                />

                <Group justify="flex-end" gap="sm">
                    <Button variant="light" onClick={onClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button
                        color="red"
                        onClick={handleConfirmar}
                        loading={loading}
                        disabled={!confirmacionValida}
                    >
                        Anular abono
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
