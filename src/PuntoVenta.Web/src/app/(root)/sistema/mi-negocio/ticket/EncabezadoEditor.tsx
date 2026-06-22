"use client";

import {
    ELEMENTO_ENCABEZADO_LABELS,
    ELEMENTO_ENCABEZADO_TEXTO_MAX,
    MAX_ELEMENTOS_TEXTO_ENCABEZADO,
    NEGOCIO_TICKET_CONFIG_FIELDS,
} from "@lib/constants/negocio-ticket-config.constants";
import type { ActualizarNegocioTicketConfigFormValues } from "@lib/types/impresion.types";
import {
    ActionIcon,
    Badge,
    Button,
    Group,
    Stack,
    Switch,
    Text,
    TextInput,
    UnstyledButton,
} from "@mantine/core";
import type { UseFormReturnType } from "@mantine/form";
import { IconGripVertical, IconPlus, IconTrash } from "@tabler/icons-react";
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

const FIELD = NEGOCIO_TICKET_CONFIG_FIELDS.ELEMENTOS_ENCABEZADO;

type ElementoValue =
    ActualizarNegocioTicketConfigFormValues[typeof FIELD][number];

interface ItemProps {
    id: string;
    form: UseFormReturnType<ActualizarNegocioTicketConfigFormValues>;
    pos: number;
    elemento: ElementoValue;
    puedeEditar: boolean;
    onEliminar: () => void;
}

function ElementoEncabezadoItem({
    id,
    form,
    pos,
    elemento,
    puedeEditar,
    onEliminar,
}: ItemProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        setActivatorNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({ id, disabled: !puedeEditar });

    const esTexto = elemento.tipo === "Texto";

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
                aria-label="Reordenar elemento (arrastrar)"
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

            <div className="flex min-w-0 flex-1 flex-wrap items-center gap-x-md gap-y-xs">
                <Group gap="xs" align="center" className="min-w-40">
                    <Text size="sm" fw={500}>
                        {ELEMENTO_ENCABEZADO_LABELS[elemento.tipo] ??
                            elemento.tipo}
                    </Text>
                    {esTexto && (
                        <Badge size="xs" variant="light" color="gray">
                            personalizado
                        </Badge>
                    )}
                </Group>

                {esTexto && (
                    <TextInput
                        className="min-w-0 flex-1"
                        placeholder="Texto a mostrar"
                        maxLength={ELEMENTO_ENCABEZADO_TEXTO_MAX}
                        aria-label="Texto del elemento"
                        key={form.key(`${FIELD}.${pos}.textoLibre`)}
                        disabled={!puedeEditar}
                        {...form.getInputProps(`${FIELD}.${pos}.textoLibre`)}
                    />
                )}
            </div>

            <Switch
                aria-label="Mostrar"
                className="self-center"
                key={form.key(`${FIELD}.${pos}.visible`)}
                disabled={!puedeEditar}
                {...form.getInputProps(`${FIELD}.${pos}.visible`, {
                    type: "checkbox",
                })}
            />

            {esTexto && (
                <ActionIcon
                    variant="light"
                    color="red"
                    aria-label="Eliminar elemento"
                    className="self-center"
                    disabled={!puedeEditar}
                    onClick={onEliminar}
                >
                    <IconTrash size={16} />
                </ActionIcon>
            )}
        </div>
    );
}

interface Props {
    form: UseFormReturnType<ActualizarNegocioTicketConfigFormValues>;
    puedeEditar: boolean;
}

export function EncabezadoEditor({ form, puedeEditar }: Props) {
    const sensors = useSensors(
        useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
        useSensor(KeyboardSensor, {
            coordinateGetter: sortableKeyboardCoordinates,
        }),
    );

    const elementos = form.getValues()[FIELD];
    const textos = elementos.filter((e) => e.tipo === "Texto").length;
    const limiteTextos = textos >= MAX_ELEMENTOS_TEXTO_ENCABEZADO;

    function onDragEnd(event: DragEndEvent) {
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        const lista = form.getValues()[FIELD];
        const from = lista.findIndex((e) => e._key === active.id);
        const to = lista.findIndex((e) => e._key === over.id);
        if (from === -1 || to === -1) return;
        form.reorderListItem(FIELD, { from, to });
    }

    function agregarTexto() {
        form.insertListItem(FIELD, {
            _key: crypto.randomUUID(),
            tipo: "Texto",
            visible: true,
            textoLibre: "",
        });
    }

    function eliminar(pos: number) {
        form.removeListItem(FIELD, pos);
    }

    return (
        <Stack gap="sm">
            <Group justify="space-between" align="center">
                <Stack gap={2}>
                    <Text fw={600} size="sm">
                        Encabezado del ticket
                    </Text>
                    <Text size="xs" c="dimmed">
                        Ordená y mostrá/ocultá los datos del negocio en el
                        encabezado del ticket impreso. Solo aplica al ticket
                        térmico.
                    </Text>
                </Stack>
                <Button
                    variant="light"
                    size="xs"
                    leftSection={<IconPlus size={14} />}
                    onClick={agregarTexto}
                    disabled={!puedeEditar || limiteTextos}
                >
                    Agregar texto
                </Button>
            </Group>

            <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragEnd={onDragEnd}
            >
                <SortableContext
                    items={elementos.map((e) => e._key)}
                    strategy={verticalListSortingStrategy}
                >
                    {elementos.map((elemento, pos) => (
                        <ElementoEncabezadoItem
                            key={elemento._key}
                            id={elemento._key}
                            form={form}
                            pos={pos}
                            elemento={elemento}
                            puedeEditar={puedeEditar}
                            onEliminar={() => eliminar(pos)}
                        />
                    ))}
                </SortableContext>
            </DndContext>

            {limiteTextos && (
                <Text size="xs" c="dimmed">
                    Máximo {MAX_ELEMENTOS_TEXTO_ENCABEZADO} elementos de texto.
                </Text>
            )}
        </Stack>
    );
}
