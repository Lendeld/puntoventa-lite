"use client";

import { EstadoActivoToggle } from "@/components/ui/table/EstadoActivoToggle";
import { AppNotifier } from "@components/ui/AppNotifier";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import type { ActionResult } from "@lib/types/base.types";
import {
    Alert,
    Badge,
    Button,
    Group,
    Paper,
    ScrollArea,
    SegmentedControl,
    Select,
    Skeleton,
    Stack,
    Text,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import {
    IconAlertCircle,
    IconLock,
    IconPencil,
    IconPlus,
    IconSearch,
} from "@tabler/icons-react";
import { useMemo, useState } from "react";

export type NaturalezaCatalogo = "Entrada" | "Salida";

export interface CatalogoCrudItem {
    id: string;
    codigo: string;
    detalle: string;
    naturaleza: NaturalezaCatalogo;
    esSistema: boolean;
    activo: boolean;
    fechaModificacion?: string | null;
}

export interface CatalogoCrudFormValues extends Record<string, unknown> {
    codigo: string;
    detalle: string;
    naturaleza: NaturalezaCatalogo;
}

interface Props {
    title: string;
    description: string;
    errorMessage: string;
    emptyMessage: string;
    entidadLabel: string;
    items: CatalogoCrudItem[] | undefined;
    loading: boolean;
    isError: boolean;
    puedeCrear: boolean;
    puedeEditar: boolean;
    puedeToggle: boolean;
    onCrear: (values: CatalogoCrudFormValues) => Promise<ActionResult>;
    onActualizar: (
        id: string,
        values: CatalogoCrudFormValues & { activo: boolean },
    ) => Promise<ActionResult>;
    onToggle: (id: string) => Promise<ActionResult>;
    onSuccess: () => Promise<void>;
}

type FiltroEstado = "todos" | "activos" | "inactivos";

const CODIGO_MAX = 30;
const DETALLE_MAX = 255;

const NATURALEZA_OPCIONES = [
    { value: "Entrada", label: "Entrada" },
    { value: "Salida", label: "Salida" },
];

function FormularioCatalogo({
    entidadLabel,
    item,
    onCrear,
    onActualizar,
    onSuccess,
}: {
    entidadLabel: string;
    item: CatalogoCrudItem | null;
    onCrear: Props["onCrear"];
    onActualizar: Props["onActualizar"];
    onSuccess: () => Promise<void>;
}) {
    const esEdicion = item !== null;
    const esSistema = item?.esSistema ?? false;

    const form = useForm<CatalogoCrudFormValues>({
        initialValues: {
            codigo: item?.codigo ?? "",
            detalle: item?.detalle ?? "",
            naturaleza: item?.naturaleza ?? "Entrada",
        },
        validate: {
            codigo: (v) =>
                v.trim().length === 0
                    ? "El código es requerido."
                    : v.trim().length > CODIGO_MAX
                      ? `Máximo ${CODIGO_MAX} caracteres.`
                      : null,
            detalle: (v) =>
                v.trim().length === 0
                    ? "El detalle es requerido."
                    : v.trim().length > DETALLE_MAX
                      ? `Máximo ${DETALLE_MAX} caracteres.`
                      : null,
        },
    });

    const { execute, loading, error, setError } =
        useActionHandler<CatalogoCrudFormValues>({
            form,
            onSuccess: async () => {
                await onSuccess();
                AppNotifier.success({
                    message: `${entidadLabel} guardado exitosamente.`,
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: CatalogoCrudFormValues) {
        if (esEdicion) {
            await execute(() =>
                onActualizar(item!.id, { ...values, activo: item!.activo }),
            );
        } else {
            await execute(() => onCrear(values));
        }
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Stack gap="md">
                {error && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        variant="light"
                        color="red"
                    >
                        {error}
                    </Alert>
                )}
                <TextInput
                    label="Código"
                    placeholder="Ej: PROPINA"
                    required
                    maxLength={CODIGO_MAX}
                    disabled={esSistema}
                    key={form.key("codigo")}
                    {...form.getInputProps("codigo")}
                />
                <TextInput
                    label="Detalle"
                    placeholder="Descripción del concepto"
                    required
                    maxLength={DETALLE_MAX}
                    key={form.key("detalle")}
                    {...form.getInputProps("detalle")}
                />
                <Select
                    label="Naturaleza"
                    data={NATURALEZA_OPCIONES}
                    allowDeselect={false}
                    disabled={esSistema}
                    key={form.key("naturaleza")}
                    {...form.getInputProps("naturaleza")}
                />
                {esSistema && (
                    <Text size="xs" c="dimmed">
                        Es un concepto del sistema: el código y la naturaleza no
                        se pueden modificar.
                    </Text>
                )}
                <Group justify="flex-end" gap="sm" mt="xs">
                    <Button
                        variant="light"
                        onClick={() => modals.closeAll()}
                        disabled={loading}
                    >
                        Cancelar
                    </Button>
                    <Button type="submit" loading={loading}>
                        {esEdicion ? "Guardar cambios" : "Crear"}
                    </Button>
                </Group>
            </Stack>
        </form>
    );
}

export function CatalogoCrudList({
    title,
    description,
    errorMessage,
    emptyMessage,
    entidadLabel,
    items,
    loading,
    isError,
    puedeCrear,
    puedeEditar,
    puedeToggle,
    onCrear,
    onActualizar,
    onToggle,
    onSuccess,
}: Props) {
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

    function abrirModal(item: CatalogoCrudItem | null) {
        modals.open({
            title: item ? `Editar ${entidadLabel}` : `Nuevo ${entidadLabel}`,
            children: (
                <FormularioCatalogo
                    entidadLabel={entidadLabel}
                    item={item}
                    onCrear={onCrear}
                    onActualizar={onActualizar}
                    onSuccess={onSuccess}
                />
            ),
        });
    }

    return (
        <Paper className="bg-theme-surface">
            <Group
                justify="space-between"
                align="flex-start"
                className="w-full border-b border-theme p-4"
            >
                <Stack gap={4}>
                    <Text fw={700} size="lg">
                        {title}
                    </Text>
                    <Text size="sm" c="dimmed">
                        {description}
                    </Text>
                </Stack>
                {puedeCrear && (
                    <Button
                        leftSection={<IconPlus size={16} />}
                        onClick={() => abrirModal(null)}
                    >
                        Nuevo
                    </Button>
                )}
            </Group>

            <Stack gap={0} className="border-b border-theme px-4 py-3">
                <Group gap="sm" align="flex-end">
                    <TextInput
                        placeholder="Buscar por detalle o código…"
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
                                    <Badge
                                        variant="light"
                                        color={
                                            item.naturaleza === "Entrada"
                                                ? "green"
                                                : "orange"
                                        }
                                        size="sm"
                                    >
                                        {item.naturaleza}
                                    </Badge>
                                    {item.esSistema && (
                                        <Badge
                                            variant="default"
                                            size="sm"
                                            leftSection={<IconLock size={11} />}
                                            classNames={{
                                                root: "bg-theme-surface-3 text-theme-text-muted border border-theme-border",
                                            }}
                                        >
                                            Sistema
                                        </Badge>
                                    )}
                                    <Badge
                                        variant="dot"
                                        color={item.activo ? "green" : "gray"}
                                        size="sm"
                                        classNames={
                                            item.activo
                                                ? undefined
                                                : {
                                                      root: "text-theme-text-muted border-theme-border",
                                                  }
                                        }
                                    >
                                        {item.activo ? "Activo" : "Inactivo"}
                                    </Badge>
                                </Group>
                            </Stack>
                            <Group gap="xs" wrap="nowrap">
                                {puedeEditar && (
                                    <Button
                                        variant="light"
                                        size="xs"
                                        leftSection={<IconPencil size={14} />}
                                        onClick={() => abrirModal(item)}
                                    >
                                        Editar
                                    </Button>
                                )}
                                {!item.esSistema && (
                                    <EstadoActivoToggle
                                        activo={item.activo}
                                        disabled={!puedeToggle}
                                        onToggle={() => onToggle(item.id)}
                                        onSuccess={onSuccess}
                                        confirmTitle="Cambiar estado"
                                        confirmMessage={`Se ${item.activo ? "desactivará" : "activará"} ${item.detalle}.`}
                                        confirmLabel="Sí, continuar"
                                        confirmVariant={
                                            item.activo ? "danger" : "default"
                                        }
                                    />
                                )}
                            </Group>
                        </Group>
                    ))}
            </ScrollArea>
        </Paper>
    );
}
