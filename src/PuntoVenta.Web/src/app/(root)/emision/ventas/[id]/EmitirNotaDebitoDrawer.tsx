"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { emitirNotaDebitoAction } from "@lib/actions/ventas.actions";
import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import { imprimirTicketAuto } from "@lib/printing/imprimir-ticket";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    Alert,
    Button,
    Drawer,
    Group,
    NumberInput,
    Stack,
    Table,
    Text,
    Textarea,
} from "@mantine/core";
import { useRouter } from "next/navigation";
import { useMemo, useRef, type RefObject, type SetStateAction } from "react";

interface Props {
    opened: boolean;
    onClose: () => void;
    documento: DocumentoVentaDto;
}

// Mantine NumberInput emite string en estados intermedios (ej. "2." al teclear
// el separador decimal). Convertir a número preservando el valor parcial evita
// que el campo se resetee a 0 y permite seguir tecleando decimales.
function aNumero(v: number | string): number {
    if (typeof v === "number") return v;
    const n = parseFloat(v);
    return Number.isFinite(n) ? n : 0;
}

interface LineaCargo {
    productoId: string;
    cantidad: number;
    porcentajeIva: number;
    descripcion: string;
    codigo: string;
    // Monto adicional a cobrar por esta línea (sin IVA). 0 = no aporta.
    montoCargo: number;
}

// Construye las líneas elegibles (con producto) en su estado inicial de cargo 0.
function construirLineas(documento: DocumentoVentaDto): LineaCargo[] {
    return documento.lineas.flatMap((l) => {
            if (l.productoId === null) return [];
            const subtotalOrigen = l.cantidad * l.precioUnitario - l.montoDescuento;
            const porcentajeIva =
                subtotalOrigen > 0 && l.montoImpuesto > 0
                    ? l.montoImpuesto / subtotalOrigen
                    : 0;
            return [{
                productoId: l.productoId!,
                cantidad: l.cantidad,
                porcentajeIva,
                descripcion: l.descripcion,
                codigo: l.codigo,
                montoCargo: 0,
            }];
        });
}

// Outer: el Drawer persiste para la animación; Mantine desmonta el contenido al
// cerrar (keepMounted=false), por lo que el form arranca limpio en cada apertura
// vía inicializadores de useState — sin efecto de reset.
export default function EmitirNotaDebitoDrawer({ opened, onClose, documento }: Props) {
    const loadingRef = useRef(false);
    return (
        <Drawer
            opened={opened}
            onClose={() => {
                if (!loadingRef.current) onClose();
            }}
            position="right"
            size="xl"
            title="Emitir nota de débito"
        >
            <NotaDebitoFormContent
                documento={documento}
                onClose={onClose}
                loadingRef={loadingRef}
            />
        </Drawer>
    );
}

// El formulario representa una transacción única con líneas, total y cobro opcional acoplados por validación.
// react-doctor-disable-next-line react-doctor/no-giant-component
function NotaDebitoFormContent({
    documento,
    onClose,
    loadingRef,
}: {
    documento: DocumentoVentaDto;
    onClose: () => void;
    loadingRef: RefObject<boolean>;
}) {
    const { push, refresh } = useRouter();
    const [
        {
            razon,
            observaciones,
            lineas,
            loading,
            errorGlobal,
        },
        patchState,
    ] = usePatchReducer(() => ({
        razon: "",
        observaciones: "",
        lineas: construirLineas(documento),
        loading: false,
        errorGlobal: null as string | null,
    }));
    const setRazon = (razon: string) => patchState({ razon });
    const setObservaciones = (observaciones: string) => patchState({ observaciones });
    const setLineas = (action: SetStateAction<LineaCargo[]>) =>
        patchState((state) => ({
            lineas: typeof action === "function" ? action(state.lineas) : action,
        }));
    const setLoading = (loading: boolean) => patchState({ loading });
    const setErrorGlobal = (errorGlobal: string | null) => patchState({ errorGlobal });

    // El outer lee este ref para bloquear el cierre mientras se emite.
    loadingRef.current = loading;

    const totales = useMemo(() => {
        let subtotal = 0;
        let impuesto = 0;
        for (const l of lineas) {
            subtotal += l.montoCargo;
            impuesto += l.montoCargo * l.porcentajeIva;
        }
        return { subtotal, impuesto, total: subtotal + impuesto };
    }, [lineas]);

    const lineasAportan = lineas.filter((l) => l.montoCargo > 0);

    function actualizarCargo(productoId: string, monto: number) {
        setLineas((prev) =>
            prev.map((l) =>
                l.productoId === productoId ? { ...l, montoCargo: Math.max(monto, 0) } : l,
            ),
        );
    }

    async function handleEmitir() {
        setErrorGlobal(null);
        const notificarValidacion = (msg: string) => {
            setErrorGlobal(msg);
            AppNotifier.warning({ message: msg });
        };
        if (lineasAportan.length === 0) {
            notificarValidacion("Indica el monto a cobrar en al menos una línea.");
            return;
        }

        const lineasPayload = lineasAportan.map((l) => ({
            productoId: l.productoId,
            cantidad: l.cantidad,
            // Precio unitario = monto cargo / cantidad (el backend recalcula totales).
            precioUnitario: l.cantidad > 0 ? l.montoCargo / l.cantidad : l.montoCargo,
            montoDescuento: 0,
            descripcion: l.descripcion,
        }));

        setLoading(true);
        const result = await emitirNotaDebitoAction({
            documentoOrigenId: documento.id,
            lineas: lineasPayload,
            razon,
            observaciones: observaciones || null,
        });
        setLoading(false);

        if (result.errors) {
            const msg = resolveApiErrorMessage(result.errors, {
                fallback: "No fue posible emitir la nota de débito.",
            });
            setErrorGlobal(msg);
            if (result.status >= 500) {
                AppNotifier.error({ message: msg });
            } else {
                AppNotifier.warning({ message: msg });
            }
            return;
        }

        AppNotifier.success({ message: "Nota de débito emitida." });

        if (result.data?.id) {
            void imprimirTicketAuto(result.data.id).then((r) => {
                if (r.status === "error") {
                    AppNotifier.error({
                        title: "No se pudo imprimir el ticket",
                        message: r.message,
                    });
                }
            });
        }

        onClose();
        if (result.data?.id) {
            push(`/emision/ventas/${result.data.id}`);
            return;
        }
        refresh();
    }

    return (
        <Stack gap="md">
                {errorGlobal && (
                    <Alert color="red" variant="light">
                        {errorGlobal}
                    </Alert>
                )}

                <Alert color="blue" variant="light" title="Cargo adicional">
                    La nota de débito suma un cargo a la factura {documento.consecutivo}. El monto
                    se agrega a lo que debe el cliente. Para reversar la factura con sus cargos,
                    emite una nota de crédito contra la factura y otra contra cada ND por separado.
                </Alert>

                <Stack gap={4}>
                    <Text size="xs" tt="uppercase" fw={700} c="dimmed">
                        Líneas a cargar
                    </Text>
                    <Table withColumnBorders striped highlightOnHover>
                        <Table.Thead>
                            <Table.Tr>
                                <Table.Th>Producto</Table.Th>
                                <Table.Th ta="right">Monto a cobrar (sin IVA)</Table.Th>
                                <Table.Th ta="right">Subtotal ND</Table.Th>
                            </Table.Tr>
                        </Table.Thead>
                        <Table.Tbody>
                            {lineas.map((l) => (
                                <Table.Tr key={l.productoId}>
                                    <Table.Td>
                                        <Stack gap={0}>
                                            <Text size="sm" fw={600}>
                                                {l.descripcion}
                                            </Text>
                                            <Text c="dimmed" size="xs">
                                                {l.codigo}
                                            </Text>
                                        </Stack>
                                    </Table.Td>
                                    <Table.Td ta="right" w={180}>
                                        <NumberInput
                                            size="xs"
                                            value={l.montoCargo}
                                            min={0}
                                            decimalScale={2}
                                            allowNegative={false}
                                            allowLeadingZeros={false}
                                            onChange={(v) =>
                                                actualizarCargo(l.productoId, aNumero(v))
                                            }
                                        />
                                    </Table.Td>
                                    <Table.Td ta="right">
                                        {formatMonedaPorCodigo(
                                            l.montoCargo * (1 + l.porcentajeIva),
                                            documento.monedaCodigo,
                                        )}
                                    </Table.Td>
                                </Table.Tr>
                            ))}
                        </Table.Tbody>
                    </Table>
                    <Stack gap={4} pt="xs">
                        <Group justify="space-between">
                            <Text c="dimmed" size="sm">
                                Subtotal ND
                            </Text>
                            <Text size="sm">
                                {formatMonedaPorCodigo(totales.subtotal, documento.monedaCodigo)}
                            </Text>
                        </Group>
                        <Group justify="space-between">
                            <Text c="dimmed" size="sm">
                                IVA ND
                            </Text>
                            <Text size="sm">
                                {formatMonedaPorCodigo(totales.impuesto, documento.monedaCodigo)}
                            </Text>
                        </Group>
                        <Group justify="space-between">
                            <Text fw={700}>Total ND</Text>
                            <Text fw={800}>
                                {formatMonedaPorCodigo(totales.total, documento.monedaCodigo)}
                            </Text>
                        </Group>
                    </Stack>
                </Stack>

                <Textarea
                    label="Razón"
                    description="Aparece en el campo de referencia del comprobante Hacienda."
                    value={razon}
                    onChange={(e) => setRazon(e.currentTarget.value)}
                    maxLength={180}
                />

                <Textarea
                    label="Observaciones"
                    value={observaciones}
                    onChange={(e) => setObservaciones(e.currentTarget.value)}
                />

                <Group justify="flex-end" mt="md">
                    <Button variant="light" onClick={onClose} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button loading={loading} onClick={handleEmitir}>
                        Emitir nota
                    </Button>
                </Group>
        </Stack>
    );
}
