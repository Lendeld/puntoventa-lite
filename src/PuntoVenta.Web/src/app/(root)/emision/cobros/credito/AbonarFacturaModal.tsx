"use client";

import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import { MEDIO_PAGO_EFECTIVO } from "@lib/constants/ventas.constants";
import {
    Alert,
    Button,
    Group,
    Modal,
    NumberInput,
    Select,
    Stack,
    Text,
    TextInput,
    Textarea,
} from "@mantine/core";
import { DateTimePicker } from "@mantine/dates";
import { IconAlertCircle, IconCash } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { AppNotifier } from "@components/ui/AppNotifier";
import { registrarAbonoFacturaAction } from "@lib/actions/ventas.actions";
import { useMediosPagoActivosQuery } from "@lib/hooks/useMediosPagoActivosQuery";
import type { FacturaCreditoResumenDto } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { AbonoResultadoModal } from "@pages/emision/ventas/[id]/AbonoResultadoModal";
import dayjs from "dayjs";

interface Props {
    factura: FacturaCreditoResumenDto;
    onClose: () => void;
    onSuccess: () => void;
}

export function AbonarFacturaModal({ factura, onClose, onSuccess }: Props) {
    const queryClient = useQueryClient();
    const { data: mediosPago } = useMediosPagoActivosQuery();
    // Seed inicial del form desde la prop (el usuario luego edita el monto).
    // react-doctor-disable-next-line react-doctor/no-derived-useState
    const [
        {
            monto,
            medioPagoCodigo,
            referencia,
            observacion,
            fechaPago,
            loading,
            error,
            pagoIdRegistrado,
        },
        patchState,
    ] = usePatchReducer(() => ({
        monto: factura.saldoPendiente as number | "",
        medioPagoCodigo: MEDIO_PAGO_EFECTIVO as string,
        referencia: "",
        observacion: "",
        fechaPago: dayjs().toISOString(),
        loading: false,
        error: null as string | null,
        pagoIdRegistrado: null as string | null,
    }));

    const medioPagoOptions = (mediosPago ?? []).map((mp) => ({
        value: mp.codigo,
        label: mp.detalle,
    }));

    const montoNum = typeof monto === "number" ? monto : 0;
    const montoExcede = montoNum > factura.saldoPendiente;
    const mostrarReferencia = medioPagoCodigo !== MEDIO_PAGO_EFECTIVO;

    async function handleSubmit() {
        patchState({ error: null });
        if (!montoNum || montoNum <= 0) {
            patchState({ error: "El monto debe ser mayor a cero." });
            return;
        }
        if (montoExcede) {
            patchState({ error: "El monto no puede exceder el saldo pendiente." });
            return;
        }
        if (!medioPagoCodigo) {
            patchState({ error: "Selecciona un medio de pago." });
            return;
        }

        patchState({ loading: true });
        const result = await registrarAbonoFacturaAction(factura.id, {
            monedaCodigo: "CRC",
            medioPagoCodigo,
            monto: montoNum,
            referencia: referencia || null,
            observacion: observacion || null,
            fechaPago,
        });
        patchState({ loading: false });

        if (result.status >= 400) {
            const firstError = result.errors
                ? Object.values(result.errors)[0]
                : "Error al registrar el abono.";
            patchState({ error: firstError ?? "Error al registrar el abono." });
            return;
        }

        AppNotifier.success({ message: "Abono registrado." });
        patchState({ pagoIdRegistrado: result.data?.pagoId ?? null });
    }

    function handleCerrarResultado() {
        queryClient.invalidateQueries({ queryKey: ["ventas", "credito"] });
        queryClient.invalidateQueries({
            queryKey: QUERY_KEYS.ventas.saldoCliente(factura.clienteId ?? ""),
        });
        onSuccess();
    }

    const montoAbonadoNum = typeof monto === "number" ? monto : 0;

    if (pagoIdRegistrado) {
        return (
            <AbonoResultadoModal
                opened
                facturaId={factura.id}
                pagoId={pagoIdRegistrado}
                consecutivo={factura.consecutivo}
                montoAbono={montoAbonadoNum}
                monedaCodigo="CRC"
                onClose={handleCerrarResultado}
            />
        );
    }

    return (
        <Modal
            opened
            onClose={onClose}
            title={`Abonar factura ${factura.consecutivo ?? ""}`}
            size="lg"
        >
            <Stack gap="md">
                <Group justify="space-between">
                    <Stack gap={2}>
                        <Text size="sm" c="dimmed">{factura.clienteNombre}</Text>
                        <Text size="xs" c="dimmed">{factura.clienteIdentificacion ?? ""}</Text>
                    </Stack>
                    <Stack gap={2} align="flex-end">
                        <Text size="xs" c="dimmed">Saldo pendiente</Text>
                        <Text fw={700} c="orange">
                            {formatMonedaPorCodigo(factura.saldoPendiente, "CRC")}
                        </Text>
                    </Stack>
                </Group>

                {error && (
                    <Alert color="orange" variant="light" icon={<IconAlertCircle size={16} />}>
                        {error}
                    </Alert>
                )}

                <DateTimePicker
                    label="Fecha informativa del abono"
                    value={dayjs(fechaPago).toDate()}
                    onChange={(value) =>
                        patchState({
                            fechaPago: value ? dayjs(value).toISOString() : dayjs().toISOString(),
                        })
                    }
                    valueFormat="DD/MM/YYYY hh:mm A"
                    locale="es"
                    timePickerProps={{ format: "12h" }}
                    maxDate={new Date()}
                    required
                />

                <NumberInput
                    label="Monto a abonar"
                    placeholder="0.00"
                    value={monto}
                    onChange={(value) => patchState({ monto: typeof value === "number" ? value : "" })}
                    min={0}
                    max={factura.saldoPendiente}
                    decimalScale={2}
                    thousandSeparator=","
                    required
                    error={montoExcede ? "No puede exceder el saldo pendiente" : undefined}
                />

                <Select
                    label="Medio de pago"
                    data={medioPagoOptions}
                    value={medioPagoCodigo}
                    onChange={(value) => patchState({ medioPagoCodigo: value ?? MEDIO_PAGO_EFECTIVO })}
                    required
                />

                {mostrarReferencia && (
                    <TextInput
                        label="Referencia"
                        placeholder="Número de transferencia, cheque, etc."
                        value={referencia}
                        onChange={(event) => patchState({ referencia: event.currentTarget.value })}
                        maxLength={100}
                    />
                )}

                <Textarea
                    label="Observación"
                    placeholder="Notas internas (opcional)"
                    value={observacion}
                    onChange={(event) => patchState({ observacion: event.currentTarget.value })}
                    maxLength={255}
                    rows={2}
                />

                <Group justify="flex-end" gap="sm">
                    <Button variant="light" onClick={onClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button onClick={handleSubmit} loading={loading} leftSection={<IconCash size={16} />}>
                        Registrar abono
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
