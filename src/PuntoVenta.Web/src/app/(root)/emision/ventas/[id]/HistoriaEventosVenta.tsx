"use client";

import type { DocumentoVentaEventoDto } from "@lib/types/ventas.types";
import { useEventosVentaQuery } from "@lib/hooks/useEventosVentaQuery";
import {
    Alert,
    Button,
    Card,
    Group,
    Loader,
    Stack,
    Text,
    ThemeIcon,
    Timeline,
    Tooltip,
} from "@mantine/core";
import {
    IconActivity,
    IconAlertTriangle,
    IconArrowsExchange,
    IconBan,
    IconBookmark,
    IconBookmarkOff,
    IconCalendarPlus,
    IconCash,
    IconCashOff,
    IconCircleCheck,
    IconCloudUpload,
    IconFilePlus,
    IconFileText,
    IconHistory,
    IconMail,
    IconMailX,
    IconNotes,
    IconReceipt2,
    IconReceiptRefund,
    IconReceiptTax,
    IconRefresh,
    IconReload,
} from "@tabler/icons-react";
import { formatDate } from "@lib/utils/date.utils";
import { useState } from "react";

const ICON_MAP: Record<
    string,
    React.ComponentType<{ size?: number | string }>
> = {
    IconFilePlus,
    IconNotes,
    IconReceipt2,
    IconBan,
    IconReceiptRefund,
    IconReceiptTax,
    IconBookmark,
    IconBookmarkOff,
    IconArrowsExchange,
    IconCalendarPlus,
    IconCash,
    IconCashOff,
    IconCircleCheck,
    IconCloudUpload,
    IconAlertTriangle,
    IconFileText,
    IconMail,
    IconMailX,
    IconRefresh,
};

const EVENTO_LABELS: Record<string, string> = {
    FacturaEmitida: "Factura Emitida",
    FacturaEmitidaDesdeProforma: "Factura Emitida desde Proforma",
    ApartadoCreado: "Apartado Creado",
    ApartadoCancelado: "Apartado Cancelado",
    ApartadoConvertidoAFactura: "Apartado Convertido a Factura",
    AbonoRegistrado: "Abono Registrado",
    AbonoRevertido: "Abono Revertido",
    SaldoCancelado: "Saldo Cancelado",
    VencimientoExtendido: "Vencimiento Extendido",
    NotaCreditoEmitida: "Nota de Crédito Emitida",
    NotaCreditoAplicada: "Nota de Crédito Aplicada",
    NotaDebitoEmitida: "Nota de Débito Emitida",
    NotaDebitoAplicada: "Nota de Débito Aplicada",
};

function resolveIcon(
    name: string | null,
): React.ComponentType<{ size?: number | string }> {
    if (!name) return IconActivity;
    return ICON_MAP[name] ?? IconActivity;
}

export function humanizarEventoVenta(valor: string | null | undefined): string {
    const texto = valor?.trim();
    if (!texto) return "Evento";
    if (EVENTO_LABELS[texto]) return EVENTO_LABELS[texto];
    if (/\s/.test(texto)) return texto;
    return texto
        .replace(/([a-záéíóúñ])([A-ZÁÉÍÓÚÑ])/g, "$1 $2")
        .replace(/([A-ZÁÉÍÓÚÑ]+)([A-ZÁÉÍÓÚÑ][a-záéíóúñ])/g, "$1 $2");
}

interface ItemProps {
    evento: DocumentoVentaEventoDto;
}

function EventoItem({ evento }: ItemProps) {
    const Icon = resolveIcon(evento.iconoSugerido);
    const color = evento.colorSugerido ?? "gray";

    return (
        <Timeline.Item
            bullet={
                <ThemeIcon color={color} variant="light" radius="xl" size={28}>
                    <Icon size={16} />
                </ThemeIcon>
            }
            title={
                <Text fw={600} size="sm">
                    {humanizarEventoVenta(evento.tipoNombre || evento.tipoCodigo)}
                </Text>
            }
        >
            <Text size="sm" c="dimmed" mt={2}>
                {evento.resumen}
            </Text>
            <Group gap="xs" mt={4}>
                <Tooltip label={formatDate(evento.ocurridoEn, "datetime-full")}>
                    <Text size="xs" c="dimmed">
                        {formatDate(evento.ocurridoEn, "datetime")}
                    </Text>
                </Tooltip>
                {evento.usuarioNombre && (
                    <Text size="xs" c="dimmed">
                        · {evento.usuarioNombre}
                    </Text>
                )}
            </Group>
        </Timeline.Item>
    );
}

interface Props {
    ventaId: string;
    pageSize?: number;
}

export function HistoriaEventosVenta({ ventaId, pageSize = 50 }: Props) {
    // Seed inicial de paginación desde el default; take luego crece con "ver más".
    // react-doctor-disable-next-line react-doctor/no-derived-useState
    const [take, setTake] = useState(pageSize);
    const { data, isLoading, isError, isFetching, error, refetch } =
        useEventosVentaQuery(ventaId, {
            take,
            skip: 0,
        });

    const items = data?.items ?? [];
    const total = data?.total ?? 0;

    return (
        <Card
            radius="lg"
            p="lg"
           
        >
            <Stack gap="md">
                <Group justify="space-between" align="flex-start">
                    <Group gap="xs">
                        <ThemeIcon variant="light" color="accentPV">
                            <IconHistory size={18} />
                        </ThemeIcon>
                        <Stack gap={2}>
                            <Text fw={700}>Historial de eventos</Text>
                            <Text size="sm" c="dimmed">
                                Línea de tiempo cronológica de todo lo que pasó
                                con este documento.
                            </Text>
                        </Stack>
                    </Group>
                    <Button
                        variant="light"
                        size="compact-sm"
                        leftSection={<IconReload size={14} />}
                        onClick={() => refetch()}
                        loading={isFetching && !isLoading}
                    >
                        Refrescar
                    </Button>
                </Group>

                {isLoading ? (
                    <Group justify="center" py="xl">
                        <Loader size="sm" />
                        <Text size="sm" c="dimmed">
                            Cargando historial…
                        </Text>
                    </Group>
                ) : isError ? (
                    <Alert color="red" variant="light">
                        No fue posible cargar el historial: {String(error)}
                    </Alert>
                ) : items.length === 0 ? (
                    <Text c="dimmed" size="sm" ta="center" py="md">
                        Aún no hay eventos registrados para esta venta.
                    </Text>
                ) : (
                    <Stack gap="md">
                        <Text size="xs" c="dimmed">
                            {total} {total === 1 ? "evento" : "eventos"}{" "}
                            registrados
                        </Text>
                        <Timeline
                            active={items.length - 1}
                            bulletSize={28}
                            lineWidth={2}
                            reverseActive
                        >
                            {items.map((evento) => (
                                <EventoItem key={evento.id} evento={evento} />
                            ))}
                        </Timeline>
                        {items.length < total && (
                            <Group justify="center">
                                <Button
                                    variant="light"
                                    size="xs"
                                    onClick={() => setTake((t) => t + pageSize)}
                                >
                                    Cargar más ({total - items.length}{" "}
                                    restantes)
                                </Button>
                            </Group>
                        )}
                    </Stack>
                )}
            </Stack>
        </Card>
    );
}
