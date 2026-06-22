"use client";

import {
    ALINEACION_LINEA_PIE_OPTIONS,
    CONFIG_NOMBRE_MAX,
    LINEA_PIE_TEXTO_MAX,
    MAX_CONFIGS_POR_DESTINO,
    MAX_LINEAS,
    NEGOCIO_TICKET_CONFIG_FIELDS,
    TIPO_DOCUMENTO_LINEA_PIE_OPTIONS,
} from "@lib/constants/negocio-ticket-config.constants";
import type { ActualizarNegocioTicketConfigFormValues } from "@lib/types/impresion.types";
import {
    ActionIcon,
    Alert,
    Badge,
    Button,
    Collapse,
    Group,
    MultiSelect,
    Select,
    Stack,
    Switch,
    Text,
    TextInput,
    Tooltip,
    UnstyledButton,
} from "@mantine/core";
import type { UseFormReturnType } from "@mantine/form";
import {
    IconChevronDown,
    IconCopy,
    IconGripVertical,
    IconInfoCircle,
    IconPlus,
    IconTrash,
} from "@tabler/icons-react";
import {
    closestCenter,
    DndContext,
    type DragEndEvent,
    KeyboardSensor,
    PointerSensor,
    useSensor,
    useSensors,
} from "@dnd-kit/core";
import {
    SortableContext,
    sortableKeyboardCoordinates,
    useSortable,
    verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useState } from "react";

const FIELD = NEGOCIO_TICKET_CONFIG_FIELDS.CONFIGURACIONES;

type ConfigValue =
    ActualizarNegocioTicketConfigFormValues[typeof FIELD][number];
type LineaValue = ConfigValue["lineas"][number];
type DestinoLinea = ConfigValue["destino"];

const GRUPOS: ReadonlyArray<{
    destino: DestinoLinea;
    label: string;
    hint: string;
}> = [
    {
        destino: "Pdf",
        label: "Comprobante PDF",
        hint: "Líneas al pie del comprobante PDF (descarga y correo).",
    },
    {
        destino: "Ticket",
        label: "Ticket impreso",
        hint: "Líneas al pie del ticket de impresora térmica.",
    },
];

const OTRO_DESTINO: Record<DestinoLinea, DestinoLinea> = {
    Pdf: "Ticket",
    Ticket: "Pdf",
};

const DESTINO_NOMBRE: Record<DestinoLinea, string> = {
    Pdf: "ticket",
    Ticket: "PDF",
};

const TIPO_LABEL = new Map(
    TIPO_DOCUMENTO_LINEA_PIE_OPTIONS.map((o) => [o.value, o.label]),
);

function tiposResumen(tipos: ConfigValue["tiposDocumento"]) {
    if (tipos.length === 0) return "Todos los documentos";
    if (tipos.length > 2) return `${tipos.length} tipos`;
    return tipos.map((t) => TIPO_LABEL.get(t) ?? t).join(", ");
}

interface LineaItemProps {
    id: string;
    form: UseFormReturnType<ActualizarNegocioTicketConfigFormValues>;
    configIndex: number;
    posLinea: number;
    puedeEditar: boolean;
    onEliminar: () => void;
}

// Fila ordenable de una línea de pie. dnd-kit exige que cada elemento sortable
// llame a useSortable, por eso es un componente propio (no inline en el map).
function LineaPieItem({
    id,
    form,
    configIndex,
    posLinea,
    puedeEditar,
    onEliminar,
}: LineaItemProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        setActivatorNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({ id, disabled: !puedeEditar });

    const base = `${FIELD}.${configIndex}.lineas.${posLinea}`;

    return (
        <div
            ref={setNodeRef}
            style={{
                transform: CSS.Transform.toString(transform),
                transition,
                zIndex: isDragging ? 10 : undefined,
            }}
            className={`relative mb-2.5 flex items-stretch gap-2 overflow-hidden rounded-md border border-theme-border p-2 transition-shadow last:mb-0 ${
                isDragging ? "bg-theme-accent-soft shadow-md" : ""
            }`}
        >
            <UnstyledButton
                component="div"
                ref={setActivatorNodeRef}
                aria-label="Reordenar línea (arrastrar)"
                className={`-my-2 -ml-2 flex items-center self-stretch border-r border-theme-border px-1 text-theme-text-muted transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent-soft ${
                    puedeEditar
                        ? "cursor-grab hover:bg-theme-accent-soft hover:text-theme-text active:cursor-grabbing"
                        : "cursor-not-allowed opacity-50"
                }`}
                {...attributes}
                {...listeners}
            >
                <IconGripVertical size={16} />
            </UnstyledButton>

            <Stack gap="xs" className="min-w-0 flex-1">
                <TextInput
                    placeholder="Ej: SINPE Móvil 8888-8888"
                    maxLength={LINEA_PIE_TEXTO_MAX}
                    key={form.key(`${base}.texto`)}
                    disabled={!puedeEditar}
                    {...form.getInputProps(`${base}.texto`)}
                />
                <Group gap="md" wrap="wrap">
                    <Select
                        aria-label="Alineación"
                        data={[...ALINEACION_LINEA_PIE_OPTIONS]}
                        allowDeselect={false}
                        className="max-w-40"
                        key={form.key(`${base}.alineacion`)}
                        disabled={!puedeEditar}
                        {...form.getInputProps(`${base}.alineacion`)}
                    />
                    <Switch
                        label="Negrita"
                        key={form.key(`${base}.negrita`)}
                        disabled={!puedeEditar}
                        {...form.getInputProps(`${base}.negrita`, {
                            type: "checkbox",
                        })}
                    />
                </Group>
            </Stack>

            <ActionIcon
                variant="light"
                color="red"
                aria-label="Eliminar línea"
                className="self-start"
                disabled={!puedeEditar}
                onClick={onEliminar}
            >
                <IconTrash size={16} />
            </ActionIcon>
        </div>
    );
}

interface Props {
    form: UseFormReturnType<ActualizarNegocioTicketConfigFormValues>;
    puedeEditar: boolean;
}

// Editor jerárquico ligado a paths dinámicos del form; fragmentarlo ocultaría las operaciones de lista.
// react-doctor-disable-next-line react-doctor/no-giant-component
export function ConfiguracionesEditor({ form, puedeEditar }: Props) {
    const [abiertas, setAbiertas] = useState<Set<string>>(new Set());
    const sensors = useSensors(
        useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
        useSensor(KeyboardSensor, {
            coordinateGetter: sortableKeyboardCoordinates,
        }),
    );
    const configs = form.getValues()[FIELD];
    const configsIndexadas = configs.map((config, index) => ({
        config,
        index,
    }));

    function toggle(key: string) {
        setAbiertas((prev) => {
            const next = new Set(prev);
            if (next.has(key)) next.delete(key);
            else next.add(key);
            return next;
        });
    }

    function agregarConfig(destino: DestinoLinea) {
        const key = crypto.randomUUID();
        form.insertListItem(FIELD, {
            _key: key,
            nombre: "",
            destino,
            tiposDocumento: [],
            lineas: [],
        });
        setAbiertas((prev) => new Set(prev).add(key));
    }

    function eliminarConfig(index: number) {
        form.removeListItem(FIELD, index);
    }

    function duplicarConfig(index: number) {
        const origen = form.getValues()[FIELD][index];
        const key = crypto.randomUUID();
        form.insertListItem(FIELD, {
            _key: key,
            // El destino opuesto puede tener sus tipos ya ocupados; se copia el nombre
            // y las líneas, y el usuario ajusta los tipos en el nuevo destino.
            nombre: origen.nombre,
            destino: OTRO_DESTINO[origen.destino],
            tiposDocumento: [],
            lineas: origen.lineas.map((linea: LineaValue) => ({
                _key: crypto.randomUUID(),
                texto: linea.texto,
                alineacion: linea.alineacion,
                negrita: linea.negrita,
            })),
        });
        setAbiertas((prev) => new Set(prev).add(key));
    }

    function agregarLinea(configIndex: number) {
        form.insertListItem(`${FIELD}.${configIndex}.lineas`, {
            _key: crypto.randomUUID(),
            texto: "",
            alineacion: "Izquierda",
            negrita: false,
        });
    }

    function eliminarLinea(configIndex: number, lineaIndex: number) {
        form.removeListItem(`${FIELD}.${configIndex}.lineas`, lineaIndex);
    }

    function reordenarLineas(configIndex: number, event: DragEndEvent) {
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        const lineas = form.getValues()[FIELD][configIndex].lineas;
        const from = lineas.findIndex((l) => l._key === active.id);
        const to = lineas.findIndex((l) => l._key === over.id);
        if (from === -1 || to === -1) return;
        form.reorderListItem(`${FIELD}.${configIndex}.lineas`, { from, to });
    }

    return (
        <Stack gap="xl">
            <Stack gap={2}>
                <Text fw={600} size="sm">
                    Configuraciones de líneas de pie
                </Text>
                <Text size="xs" c="dimmed">
                    Datos extra al final del comprobante (cuenta bancaria, SINPE,
                    redes). Cada configuración tiene un nombre y aplica a uno o
                    varios tipos de documento. Sin tipos seleccionados aplica a
                    todos.
                </Text>
            </Stack>

            {GRUPOS.map((grupo) => {
                const items = configsIndexadas.filter(
                    (x) => x.config.destino === grupo.destino,
                );
                const hayTodos = items.some(
                    (x) => x.config.tiposDocumento.length === 0,
                );
                const limiteAlcanzado =
                    items.length >= MAX_CONFIGS_POR_DESTINO;
                const noPuedeAgregar =
                    !puedeEditar || hayTodos || limiteAlcanzado;

                return (
                    <Stack gap="sm" key={grupo.destino}>
                        <Group justify="space-between" align="center">
                            <Group gap="xs" align="center">
                                <Text fw={600} size="sm">
                                    {grupo.label}
                                </Text>
                                {items.length > 0 && (
                                    <Badge size="sm" variant="light">
                                        {items.length}
                                    </Badge>
                                )}
                            </Group>
                            <Tooltip
                                label={
                                    hayTodos
                                        ? "Ya existe una configuración para todos los documentos."
                                        : limiteAlcanzado
                                          ? `Máximo ${MAX_CONFIGS_POR_DESTINO} configuraciones.`
                                          : "Agregar configuración"
                                }
                                disabled={!noPuedeAgregar || !puedeEditar}
                            >
                                <Button
                                    variant="light"
                                    size="xs"
                                    leftSection={<IconPlus size={14} />}
                                    onClick={() => agregarConfig(grupo.destino)}
                                    disabled={noPuedeAgregar}
                                >
                                    Agregar configuración
                                </Button>
                            </Tooltip>
                        </Group>

                        {items.length === 0 ? (
                            <div className="rounded-md border border-dashed border-theme-border p-4 text-center">
                                <Text size="xs" c="dimmed">
                                    {grupo.hint} Aún no hay configuraciones aquí.
                                </Text>
                            </div>
                        ) : (
                            <Stack gap="sm">
                                {items.map(({ config, index }) => {
                                    const open = abiertas.has(config._key);
                                    // Tipos ocupados por OTRAS configs del mismo destino.
                                    const ocupadosPorOtras = new Set(
                                        items.flatMap((x) =>
                                            x.index === index
                                                ? []
                                                : x.config.tiposDocumento,
                                        ),
                                    );
                                    const tiposData =
                                        TIPO_DOCUMENTO_LINEA_PIE_OPTIONS.map(
                                            (o) => ({
                                                value: o.value,
                                                label: o.label,
                                                disabled: ocupadosPorOtras.has(
                                                    o.value,
                                                ),
                                            }),
                                        );
                                    const lineas = config.lineas;
                                    const limiteLineas =
                                        lineas.length >= MAX_LINEAS;

                                    return (
                                        <div
                                            key={config._key}
                                            className="overflow-hidden rounded-md border border-theme-border"
                                        >
                                            <Group
                                                gap="xs"
                                                wrap="nowrap"
                                                className={`p-2 transition-colors ${
                                                    open
                                                        ? "bg-theme-accent-soft"
                                                        : ""
                                                }`}
                                            >
                                                <UnstyledButton
                                                    className="flex shrink-0 items-center"
                                                    aria-expanded={open}
                                                    aria-label="Expandir configuración"
                                                    onClick={() =>
                                                        toggle(config._key)
                                                    }
                                                >
                                                    <IconChevronDown
                                                        size={18}
                                                        className={`text-theme-text-muted transition-transform duration-200 ${
                                                            open
                                                                ? "rotate-180"
                                                                : ""
                                                        }`}
                                                    />
                                                </UnstyledButton>

                                                <TextInput
                                                    className="min-w-0 flex-1"
                                                    placeholder="Nombre de la configuración"
                                                    maxLength={CONFIG_NOMBRE_MAX}
                                                    key={form.key(
                                                        `${FIELD}.${index}.nombre`,
                                                    )}
                                                    disabled={!puedeEditar}
                                                    {...form.getInputProps(
                                                        `${FIELD}.${index}.nombre`,
                                                    )}
                                                />

                                                <Badge
                                                    size="sm"
                                                    variant="light"
                                                    color="gray"
                                                    className="shrink-0"
                                                >
                                                    {tiposResumen(
                                                        config.tiposDocumento,
                                                    )}
                                                </Badge>

                                                <Tooltip
                                                    label={`Duplicar en ${
                                                        DESTINO_NOMBRE[
                                                            config.destino
                                                        ]
                                                    }`}
                                                >
                                                    <ActionIcon
                                                        variant="light"
                                                        aria-label={`Duplicar en ${
                                                            DESTINO_NOMBRE[
                                                                config.destino
                                                            ]
                                                        }`}
                                                        disabled={!puedeEditar}
                                                        onClick={() =>
                                                            duplicarConfig(index)
                                                        }
                                                    >
                                                        <IconCopy size={16} />
                                                    </ActionIcon>
                                                </Tooltip>

                                                <Tooltip label="Eliminar configuración">
                                                    <ActionIcon
                                                        variant="light"
                                                        color="red"
                                                        aria-label="Eliminar configuración"
                                                        disabled={!puedeEditar}
                                                        onClick={() =>
                                                            eliminarConfig(index)
                                                        }
                                                    >
                                                        <IconTrash size={16} />
                                                    </ActionIcon>
                                                </Tooltip>
                                            </Group>

                                            <Collapse
                                                expanded={open}
                                                transitionDuration={200}
                                            >
                                                <Stack
                                                    gap="md"
                                                    className="border-t border-theme-border bg-theme-surface p-3"
                                                >
                                                    <MultiSelect
                                                        label="Tipos de documento"
                                                        description="Sin selección aplica a todos los documentos. Un tipo no puede repetirse en otra configuración de este destino."
                                                        placeholder={
                                                            config
                                                                .tiposDocumento
                                                                .length === 0
                                                                ? "Todos los documentos"
                                                                : undefined
                                                        }
                                                        data={tiposData}
                                                        clearable
                                                        key={form.key(
                                                            `${FIELD}.${index}.tiposDocumento`,
                                                        )}
                                                        disabled={!puedeEditar}
                                                        {...form.getInputProps(
                                                            `${FIELD}.${index}.tiposDocumento`,
                                                        )}
                                                    />

                                                    <Stack gap="xs">
                                                        <Group
                                                            justify="space-between"
                                                            align="center"
                                                        >
                                                            <Text
                                                                fw={600}
                                                                size="sm"
                                                            >
                                                                Líneas
                                                            </Text>
                                                            <Button
                                                                variant="light"
                                                                size="xs"
                                                                leftSection={
                                                                    <IconPlus
                                                                        size={14}
                                                                    />
                                                                }
                                                                onClick={() =>
                                                                    agregarLinea(
                                                                        index,
                                                                    )
                                                                }
                                                                disabled={
                                                                    !puedeEditar ||
                                                                    limiteLineas
                                                                }
                                                            >
                                                                Agregar línea
                                                            </Button>
                                                        </Group>

                                                        {lineas.length === 0 ? (
                                                            <Text
                                                                size="xs"
                                                                c="dimmed"
                                                            >
                                                                Sin líneas. Esta
                                                                configuración no
                                                                muestra nada al
                                                                pie.
                                                            </Text>
                                                        ) : (
                                                            <DndContext
                                                                sensors={sensors}
                                                                collisionDetection={
                                                                    closestCenter
                                                                }
                                                                onDragEnd={(
                                                                    event,
                                                                ) =>
                                                                    reordenarLineas(
                                                                        index,
                                                                        event,
                                                                    )
                                                                }
                                                            >
                                                                <SortableContext
                                                                    items={lineas.map(
                                                                        (l) =>
                                                                            l._key,
                                                                    )}
                                                                    strategy={
                                                                        verticalListSortingStrategy
                                                                    }
                                                                >
                                                                    {lineas.map(
                                                                        (
                                                                            linea: LineaValue,
                                                                            posLinea: number,
                                                                        ) => (
                                                                            <LineaPieItem
                                                                                key={
                                                                                    linea._key
                                                                                }
                                                                                id={
                                                                                    linea._key
                                                                                }
                                                                                form={
                                                                                    form
                                                                                }
                                                                                configIndex={
                                                                                    index
                                                                                }
                                                                                posLinea={
                                                                                    posLinea
                                                                                }
                                                                                puedeEditar={
                                                                                    puedeEditar
                                                                                }
                                                                                onEliminar={() =>
                                                                                    eliminarLinea(
                                                                                        index,
                                                                                        posLinea,
                                                                                    )
                                                                                }
                                                                            />
                                                                        ),
                                                                    )}
                                                                </SortableContext>
                                                            </DndContext>
                                                        )}
                                                        {limiteLineas && (
                                                            <Text
                                                                size="xs"
                                                                c="dimmed"
                                                            >
                                                                Máximo{" "}
                                                                {MAX_LINEAS}{" "}
                                                                líneas por
                                                                configuración.
                                                            </Text>
                                                        )}
                                                    </Stack>
                                                </Stack>
                                            </Collapse>
                                        </div>
                                    );
                                })}
                            </Stack>
                        )}

                        {hayTodos && (
                            <Alert
                                icon={<IconInfoCircle size={16} />}
                                color="blue"
                                variant="light"
                            >
                                Esta configuración aplica a todos los documentos,
                                por eso no se pueden agregar más en {grupo.label}.
                            </Alert>
                        )}
                    </Stack>
                );
            })}
        </Stack>
    );
}
