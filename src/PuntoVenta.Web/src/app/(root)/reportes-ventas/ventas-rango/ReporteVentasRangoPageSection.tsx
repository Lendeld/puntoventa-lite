"use client";

import { useMemo } from "react";
import {
    Alert,
    Button,
    Checkbox,
    Divider,
    Group,
    Paper,
    SegmentedControl,
    Stack,
    Text,
    TextInput,
    ThemeIcon,
} from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import {
    IconAlertTriangle,
    IconArrowLeft,
    IconCalendarStats,
    IconFileSpreadsheet,
    IconReportAnalytics,
} from "@tabler/icons-react";
import dayjs from "dayjs";
import Link from "next/link";
import { parseAsString, useQueryState } from "nuqs";
import { ROUTES } from "@lib/constants/routes.constants";

type Modo = "detallado" | "resumido";

export function ReporteVentasRangoPageSection() {
    const hoy = useMemo(() => dayjs().startOf("day").toISOString(), []);

    const [desde, setDesde] = useQueryState("desde", parseAsString.withDefault(hoy));
    const [hasta, setHasta] = useQueryState("hasta", parseAsString.withDefault(hoy));
    const [consecutivo, setConsecutivo] = useQueryState(
        "consecutivo",
        parseAsString.withDefault(""),
    );
    const [colonizar, setColonizar] = useQueryState(
        "colonizar",
        parseAsString.withDefault("1"),
    );
    const [modo, setModo] = useQueryState("modo", parseAsString.withDefault("detallado"));

    const detallado = modo !== "resumido";
    const colonizado = colonizar !== "0";
    const MAX_DIAS_RANGO = 366;
    const ordenValido = dayjs(hasta).isSame(desde) || dayjs(hasta).isAfter(desde);
    const diasRango = dayjs(hasta).startOf("day").diff(dayjs(desde).startOf("day"), "day") + 1;
    const dentroDelTope = diasRango <= MAX_DIAS_RANGO;
    const rangoValido = ordenValido && dentroDelTope;

    const advertencia = !ordenValido
        ? "La fecha hasta debe ser mayor o igual a la fecha desde."
        : !dentroDelTope
          ? `El rango no puede superar ${MAX_DIAS_RANGO} días. Reduce el período.`
          : null;

    function onDesdeChange(value: string | null) {
        if (!value) return;
        void setDesde(dayjs(value).startOf("day").toISOString());
    }

    function onHastaChange(value: string | null) {
        if (!value) return;
        void setHasta(dayjs(value).startOf("day").toISOString());
    }

    function descargarExcel() {
        const qs = new URLSearchParams({ FechaDesde: desde, FechaHasta: hasta });
        if (consecutivo.trim()) qs.set("Consecutivo", consecutivo.trim());
        qs.set("Colonizar", colonizado ? "true" : "false");
        qs.set("Detallado", detallado ? "true" : "false");
        // Disparar la descarga con un <a download> en vez de window.open: el
        // Excel se sirve con Content-Disposition: attachment, así que abrir una
        // ventana nueva (Electron) la deja en blanco. El anchor descarga directo
        // sin ventana; el nombre lo define el header del servidor.
        const a = document.createElement("a");
        a.href = `/excel/ventas/reporte-rango?${qs.toString()}`;
        a.download = "";
        a.rel = "noopener";
        document.body.appendChild(a);
        a.click();
        a.remove();
    }

    return (
        <Stack gap="lg" className="w-full">
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

            <div className="flex w-full flex-1 items-start justify-center py-2 sm:py-6">
                <Paper
                    p="xl"
                    withBorder
                    radius="lg"
                    shadow="sm"
                    className="w-full max-w-140"
                >
                    <Stack gap="lg">
                        <Stack gap="sm" align="center" className="text-center">
                            <ThemeIcon
                                size={56}
                                radius="xl"
                                variant="light"
                                color="accentPV"
                            >
                                <IconReportAnalytics size={28} />
                            </ThemeIcon>
                            <Stack gap={4} align="center">
                                <Text fw={700} size="xl">
                                    Ventas por rango
                                </Text>
                                <Text size="sm" c="dimmed" className="max-w-prose">
                                    Ventas por fecha de factura. Notas de crédito en
                                    negativo. Colonización con el tipo de cambio
                                    histórico de cada documento.
                                </Text>
                            </Stack>
                        </Stack>

                        <Divider />

                        <Stack gap="md">
                            <Group grow align="end" wrap="wrap" gap="md">
                                <DatePickerInput
                                    label="Desde"
                                    value={desde}
                                    onChange={onDesdeChange}
                                    valueFormat="DD/MM/YYYY"
                                    locale="es"
                                    leftSection={<IconCalendarStats size={15} />}
                                    clearable={false}
                                    miw={150}
                                />
                                <DatePickerInput
                                    label="Hasta"
                                    value={hasta}
                                    onChange={onHastaChange}
                                    valueFormat="DD/MM/YYYY"
                                    locale="es"
                                    leftSection={<IconCalendarStats size={15} />}
                                    clearable={false}
                                    miw={150}
                                />
                            </Group>

                            <Text size="xs" c="dimmed">
                                Máximo 366 días.
                            </Text>

                            <TextInput
                                label="Consecutivo"
                                placeholder="Opcional"
                                value={consecutivo}
                                onChange={(e) =>
                                    void setConsecutivo(e.currentTarget.value)
                                }
                            />

                            <Stack gap={6}>
                                <Text className="text-sm font-semibold text-theme-text-muted">
                                    Formato
                                </Text>
                                <SegmentedControl
                                    fullWidth
                                    value={detallado ? "detallado" : "resumido"}
                                    onChange={(v) => void setModo(v as Modo)}
                                    data={[
                                        { value: "detallado", label: "Detallado" },
                                        { value: "resumido", label: "Resumido" },
                                    ]}
                                    size="sm"
                                />
                            </Stack>

                            <Checkbox
                                label="Colonizar montos"
                                description={
                                    colonizado
                                        ? "Montos normalizados a colones (CRC)."
                                        : "Montos en la moneda original de cada documento."
                                }
                                checked={colonizado}
                                onChange={(e) =>
                                    void setColonizar(
                                        e.currentTarget.checked ? "1" : "0",
                                    )
                                }
                            />
                        </Stack>

                        {advertencia && (
                            <Alert
                                color="yellow"
                                variant="light"
                                icon={<IconAlertTriangle size={18} />}
                                title="Revisa los filtros"
                            >
                                {advertencia}
                            </Alert>
                        )}

                        <Button
                            fullWidth
                            size="md"
                            onClick={descargarExcel}
                            disabled={!rangoValido}
                            leftSection={<IconFileSpreadsheet size={18} />}
                        >
                            Exportar a Excel
                        </Button>
                    </Stack>
                </Paper>
            </div>
        </Stack>
    );
}
