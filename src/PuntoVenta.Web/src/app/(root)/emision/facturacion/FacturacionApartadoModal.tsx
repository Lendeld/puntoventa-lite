"use client";

import { Button, Group, Modal, Stack } from "@mantine/core";
import { DateTimePicker } from "@mantine/dates";
import dayjs from "dayjs";

interface Props {
    opened: boolean;
    fechaVencimiento: string;
    minDate?: string;
    error: string | null;
    loading: boolean;
    onClose: () => void;
    onFechaChange: (iso: string) => void;
    onConfirm: () => void;
}

export function FacturacionApartadoModal({
    opened,
    fechaVencimiento,
    minDate,
    error,
    loading,
    onClose,
    onFechaChange,
    onConfirm,
}: Props) {
    return (
        <Modal opened={opened} onClose={onClose} title="Crear apartado" centered>
            <Stack gap="md">
                <DateTimePicker
                    label="Fecha de vencimiento"
                    value={fechaVencimiento ? dayjs(fechaVencimiento).toDate() : null}
                    onChange={(value) =>
                        onFechaChange(value ? dayjs(value).toISOString() : "")
                    }
                    error={error}
                    valueFormat="DD/MM/YYYY hh:mm A"
                    locale="es"
                    timePickerProps={{ format: "12h" }}
                    minDate={minDate ? dayjs(minDate).toDate() : undefined}
                    submitButtonProps={{ style: { display: "none" } }}
                    clearable={false}
                />
                <Group justify="flex-end">
                    <Button variant="outline" onClick={onClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button color="orange" onClick={onConfirm} loading={loading}>
                        Crear apartado
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
