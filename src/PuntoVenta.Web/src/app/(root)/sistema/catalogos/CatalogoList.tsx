"use client";

import { EstadoActivoToggle } from "@/components/ui/table/EstadoActivoToggle";
import type { ActionResult } from "@lib/types/base.types";
import {
    Alert,
    Badge,
    Group,
    HoverCard,
    Paper,
    ScrollArea,
    SegmentedControl,
    Skeleton,
    Stack,
    Text,
    TextInput,
} from "@mantine/core";
import {
    IconAlertCircle,
    IconClockEdit,
    IconSearch,
} from "@tabler/icons-react";
import { ReactNode, useMemo, useState } from "react";

export interface BaseCatalogoItem {
    id: string;
    codigo: string;
    detalle: string;
    comentario?: string | null;
    activo: boolean;
    modificadoPor?: string | null;
    fechaModificacion?: string | null;
}

type FiltroEstado = "todos" | "activos" | "inactivos";

interface Props<T extends BaseCatalogoItem> {
    title: string;
    description: string;
    errorMessage: string;
    emptyMessage: string;
    items: T[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeToggle: boolean;
    onToggle: (id: string) => Promise<ActionResult>;
    onSuccess: () => Promise<void>;
    renderExtraBadges?: (item: T) => ReactNode;
    confirmTitle?: string;
    confirmMessage?: (item: T) => string;
    confirmLabel?: string;
}

const fechaFormatter = new Intl.DateTimeFormat("es-CR", {
    dateStyle: "medium",
    timeStyle: "short",
});

function formatFecha(iso: string): string {
    return fechaFormatter.format(new Date(iso));
}

function UltimocambioHover({
    modificadoPor,
    fechaModificacion,
}: {
    modificadoPor?: string | null;
    fechaModificacion?: string | null;
}) {
    if (!modificadoPor && !fechaModificacion) return null;

    return (
        <HoverCard
            width={220}
            shadow="md"
            openDelay={200}
            closeDelay={100}
            withArrow
        >
            <HoverCard.Target>
                <IconClockEdit
                    size={15}
                    className="text-theme-text-muted cursor-default shrink-0 mt-0.5"
                />
            </HoverCard.Target>
            <HoverCard.Dropdown>
                <Stack gap={6}>
                    <Text
                        size="xs"
                        fw={600}
                        c="dimmed"
                        tt="uppercase"
                        lts={0.5}
                    >
                        Último cambio
                    </Text>
                    {fechaModificacion && (
                        <Group gap={6} align="center">
                            <Text size="xs" c="dimmed" fw={500}>
                                Fecha:
                            </Text>
                            <Text size="xs">
                                {formatFecha(fechaModificacion)}
                            </Text>
                        </Group>
                    )}
                    {modificadoPor && (
                        <Group gap={6} align="center">
                            <Text size="xs" c="dimmed" fw={500}>
                                Usuario:
                            </Text>
                            <Text size="xs">{modificadoPor}</Text>
                        </Group>
                    )}
                </Stack>
            </HoverCard.Dropdown>
        </HoverCard>
    );
}

export function CatalogoList<T extends BaseCatalogoItem>({
    title,
    description,
    errorMessage,
    emptyMessage,
    items,
    loading,
    isError,
    puedeToggle,
    onToggle,
    onSuccess,
    renderExtraBadges,
    confirmTitle = "Cambiar estado",
    confirmMessage,
    confirmLabel = "Sí, continuar",
}: Props<T>) {
    const [busqueda, setBusqueda] = useState("");
    const [filtroEstado, setFiltroEstado] = useState<FiltroEstado>("todos");

    const itemsFiltrados = useMemo(() => {
        if (!items) return [];
        const q = busqueda.trim().toLowerCase();
        return items.filter((item) => {
            const matchBusqueda =
                q === "" ||
                item.detalle.toLowerCase().includes(q) ||
                item.codigo.toLowerCase().includes(q);
            const matchEstado =
                filtroEstado === "todos" ||
                (filtroEstado === "activos" && item.activo) ||
                (filtroEstado === "inactivos" && !item.activo);
            return matchBusqueda && matchEstado;
        });
    }, [items, busqueda, filtroEstado]);

    const hayFiltros = busqueda.trim() !== "" || filtroEstado !== "todos";

    return (
        <Paper className="bg-theme-surface">
            <Stack
                justify="space-between"
                className="w-full border-b border-theme p-4"
                gap={4}
            >
                <Text fw={700} size="lg">
                    {title}
                </Text>
                <Text size="sm" c="dimmed">
                    {description}
                </Text>
            </Stack>

            <Stack gap={0} className="border-b border-theme px-4 py-3">
                <Group gap="sm" align="flex-end">
                    <TextInput
                        placeholder="Buscar por nombre o código…"
                        leftSection={<IconSearch size={15} />}
                        value={busqueda}
                        onChange={(e) => setBusqueda(e.currentTarget.value)}
                        className="flex-1"
                        size="sm"
                    />
                    <SegmentedControl
                        size="sm"
                        value={filtroEstado}
                        onChange={(v) => setFiltroEstado(v as FiltroEstado)}
                        data={[
                            { label: "Todos", value: "todos" },
                            { label: "Activos", value: "activos" },
                            { label: "Inactivos", value: "inactivos" },
                        ]}
                    />
                </Group>

                {!loading && !isError && items && hayFiltros && (
                    <Text size="xs" c="dimmed" mt={6}>
                        {itemsFiltrados.length} de {items.length} resultados
                    </Text>
                )}
            </Stack>

            <ScrollArea
                scrollbarSize={6}
                className="h-[calc(100dvh-280px)] px-4"
            >
                {loading && <Skeleton className="h-[calc(100dvh-460px)]" />}

                {!loading && isError && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        color="red"
                        variant="light"
                    >
                        {errorMessage}
                    </Alert>
                )}

                {!loading && !isError && itemsFiltrados.length === 0 && (
                    <Text c="dimmed" py="md">
                        {hayFiltros
                            ? "Sin resultados para la búsqueda."
                            : emptyMessage}
                    </Text>
                )}

                {!loading &&
                    !isError &&
                    itemsFiltrados.map((item, index) => (
                        <Group
                            key={item.id}
                            justify="space-between"
                            align="center"
                            className={`${index > 0 ? "border-t border-dashed border-theme" : ""} py-4`}
                        >
                            <Stack gap={4} className="min-w-0 flex-1 pr-4">
                                <Group gap="xs" wrap="wrap" align="center">
                                    <Text fw={700} size="md">
                                        {item.detalle}
                                    </Text>
                                    <Badge
                                        variant="light"
                                        color="accentPV"
                                        size="sm"
                                    >
                                        {item.codigo}
                                    </Badge>
                                    {renderExtraBadges?.(item)}
                                    <Badge
                                        variant="dot"
                                        color={item.activo ? "green" : "gray"}
                                        size="sm"
                                    >
                                        {item.activo ? "Activo" : "Inactivo"}
                                    </Badge>
                                    <UltimocambioHover
                                        modificadoPor={item.modificadoPor}
                                        fechaModificacion={
                                            item.fechaModificacion
                                        }
                                    />
                                </Group>
                                <Text size="sm" c="dimmed">
                                    {item.comentario?.trim() ||
                                        "Sin comentario"}
                                </Text>
                            </Stack>
                            <EstadoActivoToggle
                                activo={item.activo}
                                disabled={!puedeToggle}
                                onToggle={() => onToggle(item.id)}
                                onSuccess={onSuccess}
                                confirmTitle={confirmTitle}
                                confirmMessage={
                                    confirmMessage
                                        ? confirmMessage(item)
                                        : `Se ${item.activo ? "desactivará" : "activará"} ${item.detalle}.`
                                }
                                confirmLabel={confirmLabel}
                                confirmVariant={
                                    item.activo ? "danger" : "default"
                                }
                            />
                        </Group>
                    ))}
            </ScrollArea>
        </Paper>
    );
}
