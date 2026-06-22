"use client";

import { useCajasQuery } from "@lib/hooks/useCajasQuery";
import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import { ROUTES } from "@lib/constants/routes.constants";
import {
    Alert,
    Button,
    Divider,
    Group,
    Paper,
    Select,
    SegmentedControl,
    Stack,
    Text,
    ThemeIcon,
} from "@mantine/core";
import { DateTimePicker } from "@mantine/dates";
import {
    IconAlertTriangle,
    IconArrowLeft,
    IconCalendarStats,
    IconFileTypePdf,
    IconInfoCircle,
    IconReportMoney,
} from "@tabler/icons-react";
import dayjs from "dayjs";
import Link from "next/link";

type RangoPreset = "hoy" | "ayer" | "semana" | "mes" | "custom";

function rangoDesdePreset(preset: Exclude<RangoPreset, "custom">) {
    const hoy = dayjs();
    switch (preset) {
        case "hoy":
            return { desde: hoy.startOf("day"), hasta: hoy.endOf("day") };
        case "ayer": {
            const ayer = hoy.subtract(1, "day");
            return { desde: ayer.startOf("day"), hasta: ayer.endOf("day") };
        }
        case "semana":
            return { desde: hoy.subtract(6, "day").startOf("day"), hasta: hoy.endOf("day") };
        case "mes":
            return { desde: hoy.startOf("month"), hasta: hoy.endOf("day") };
    }
}

function formatCajaLabel(codigo: string, nombre: string) {
    return `${codigo} - ${nombre}`;
}

export function ReporteMovimientosDineroPageSection() {
    const { data: cajas = [] } = useCajasQuery();
    const [{ preset, fechaDesde, fechaHasta, cajaId }, patchState] =
        usePatchReducer(() => ({
            preset: "hoy" as RangoPreset,
            fechaDesde: dayjs().startOf("day").toISOString(),
            fechaHasta: dayjs().endOf("day").toISOString(),
            cajaId: null as string | null,
        }));

    const ordenValido =
        dayjs(fechaHasta).isSame(dayjs(fechaDesde)) ||
        dayjs(fechaHasta).isAfter(dayjs(fechaDesde));

    const cajaOpciones = [
        { value: "", label: "Todas las cajas" },
        ...cajas.map((caja) => ({
            value: caja.id,
            label: `${formatCajaLabel(caja.codigo, caja.nombre)}${caja.activo ? "" : " (inactiva)"}`,
        })),
    ];

    const cajaSeleccionada = cajaId
        ? cajaOpciones.find((caja) => caja.value === cajaId)?.label
        : "Todas las cajas";

    function aplicarPreset(value: RangoPreset) {
        patchState({ preset: value });
        if (value === "custom") return;
        const rango = rangoDesdePreset(value);
        patchState({
            fechaDesde: rango.desde.toISOString(),
            fechaHasta: rango.hasta.toISOString(),
        });
    }

    function abrirPdf() {
        if (!ordenValido) return;
        const params = new URLSearchParams({
            fechaDesde,
            fechaHasta,
        });
        if (cajaId) params.set("cajaId", cajaId);
        window.open(`/pdf/ventas/movimientos-dinero?${params.toString()}`, "_blank", "noopener");
    }

    return (
        <Stack gap="lg" className="mx-auto w-full max-w-3xl">
            <Button
                component={Link}
                href={ROUTES.REPORTES}
                variant="light"
                size="xs"
                leftSection={<IconArrowLeft size={14} />}
                className="self-start"
            >
                Volver a reportes
            </Button>

            <Group gap="md" wrap="nowrap" align="flex-start">
                <ThemeIcon size={44} radius="md" variant="light" color="accentPV">
                    <IconReportMoney size={24} />
                </ThemeIcon>
                <Stack gap={2}>
                    <Text fw={700} size="xl">
                        Movimientos de dinero
                    </Text>
                    <Text size="sm" c="dimmed">
                        Genera el PDF consolidado de entradas, salidas, medios de pago y
                        trazabilidad de abonos/anulaciones para el rango seleccionado.
                    </Text>
                </Stack>
            </Group>

            <Paper p="lg" withBorder radius="md">
                <Stack gap="lg">
                    <Stack gap={6}>
                        <Group gap={6}>
                            <IconCalendarStats size={15} className="text-theme-text-muted" />
                            <Text size="xs" tt="uppercase" fw={700} c="dimmed">
                                Rango rápido
                            </Text>
                        </Group>
                        <SegmentedControl
                            value={preset}
                            onChange={(value) => aplicarPreset(value as RangoPreset)}
                            data={[
                                { value: "hoy", label: "Hoy" },
                                { value: "ayer", label: "Ayer" },
                                { value: "semana", label: "Últimos 7 días" },
                                { value: "mes", label: "Este mes" },
                                { value: "custom", label: "Personalizado" },
                            ]}
                            fullWidth
                            size="sm"
                        />
                    </Stack>

                    <Divider />

                    <Group align="end" wrap="wrap" gap="md">
                        <DateTimePicker
                            label="Desde"
                            value={dayjs(fechaDesde).toDate()}
                            onChange={(value) => {
                                if (!value) return;
                                patchState({
                                    fechaDesde: dayjs(value).toISOString(),
                                    preset: "custom",
                                });
                            }}
                            valueFormat="DD/MM/YYYY hh:mm A"
                            locale="es"
                            timePickerProps={{ format: "12h" }}
                            submitButtonProps={{ style: { display: "none" } }}
                            clearable={false}
                            w={220}
                        />
                        <DateTimePicker
                            label="Hasta"
                            value={dayjs(fechaHasta).toDate()}
                            onChange={(value) => {
                                if (!value) return;
                                patchState({
                                    fechaHasta: dayjs(value).toISOString(),
                                    preset: "custom",
                                });
                            }}
                            valueFormat="DD/MM/YYYY hh:mm A"
                            locale="es"
                            timePickerProps={{ format: "12h" }}
                            submitButtonProps={{ style: { display: "none" } }}
                            clearable={false}
                            w={220}
                        />
                        <Select
                            label="Caja"
                            data={cajaOpciones}
                            value={cajaId ?? ""}
                            onChange={(value) => patchState({ cajaId: value && value.length > 0 ? value : null })}
                            allowDeselect={false}
                            w={260}
                        />
                    </Group>

                    {!ordenValido && (
                        <Alert
                            color="yellow"
                            variant="light"
                            icon={<IconAlertTriangle size={18} />}
                        >
                            La fecha hasta debe ser mayor o igual a la fecha desde.
                        </Alert>
                    )}

                    <Group
                        gap={8}
                        wrap="nowrap"
                        align="flex-start"
                        className="rounded-md bg-theme-surface p-3"
                    >
                        <IconInfoCircle size={16} className="mt-0.5 shrink-0 text-theme-text-muted" />
                        <Text size="xs" c="dimmed">
                            El PDF incluye movimientos entre{" "}
                            <Text component="span" fw={600} c="dimmed">
                                {dayjs(fechaDesde).format("DD/MM/YYYY hh:mm A")}
                            </Text>{" "}
                            y{" "}
                            <Text component="span" fw={600} c="dimmed">
                                {dayjs(fechaHasta).format("DD/MM/YYYY hh:mm A")}
                            </Text>{" "}
                            para{" "}
                            <Text component="span" fw={600} c="dimmed">
                                {cajaSeleccionada}
                            </Text>
                            . El documento abre en una pestaña nueva.
                        </Text>
                    </Group>

                    <Group justify="flex-end">
                        <Button
                            onClick={abrirPdf}
                            disabled={!ordenValido}
                            leftSection={<IconFileTypePdf size={16} />}
                        >
                            Generar PDF
                        </Button>
                    </Group>
                </Stack>
            </Paper>
        </Stack>
    );
}
