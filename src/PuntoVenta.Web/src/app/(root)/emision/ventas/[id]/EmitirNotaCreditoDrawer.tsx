"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { emitirNotaCreditoAction } from "@lib/actions/ventas.actions";
import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import { imprimirTicketAuto } from "@lib/printing/imprimir-ticket";
import {
    MODO_NOTA_CREDITO,
    TIPO_DOCUMENTO_VENTA,
    type ModoNotaCreditoValue,
} from "@lib/constants/ventas.constants";
import type { DocumentoVentaDto } from "@lib/types/ventas.types";
import { resolveApiErrorMessage } from "@lib/utils/apiErrors";
import { ESTADO_DOCUMENTO, formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    Alert,
    Button,
    Checkbox,
    Drawer,
    Group,
    NumberInput,
    SegmentedControl,
    Stack,
    Table,
    Text,
    Textarea,
} from "@mantine/core";
import { useRouter } from "next/navigation";
import { useMemo, useRef, useState, type RefObject, type SetStateAction } from "react";

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

interface LineaSeleccionada {
    productoId: string;
    cantidadOriginal: number;
    cantidadDisponible: number;
    cantidad: number;
    precioOriginal: number;
    subtotalOriginal: number;
    subtotalRestadoPrevio: number;
    precioUnitario: number;
    porcentajeIva: number;
    descripcion: string;
    descripcionLinea: string;
    codigo: string;
    // ¿Reintegra al inventario al emitir la NC? Default sí; el usuario lo apaga
    // para mercancía devuelta dañada o que ya no vuelve al stock.
    reintegrar: boolean;
}

// Construye las líneas seleccionables con sus defaults según el modo de NC.
function construirLineasNC(
    documento: DocumentoVentaDto,
    modo: ModoNotaCreditoValue,
): LineaSeleccionada[] {
    return documento.lineas.flatMap((l) => {
            if (l.productoId === null) return [];
            // Inferimos % IVA de la línea origen: impuesto / subtotal. Se usa el
            // subtotal almacenado (5 dec), no recomputado desde precio.
            const porcentajeIva =
                l.subtotal > 0 && l.montoImpuesto > 0
                    ? l.montoImpuesto / l.subtotal
                    : 0;
            // Cantidad disponible para devolver = original − ya devuelto físicamente.
            // Corrige monto no consume cantidad (no devuelve stock).
            const cantidadDisponible = Math.max(
                0,
                l.cantidad - l.cantidadDevueltaEnNotasCredito,
            );
            // Precio efectivo = saldo subtotal de la línea / cantidad original.
            // Garantiza que Devolución y Anulación usen el precio neto tras
            // NCs previas, no el original.
            const subtotalDisponibleLinea = Math.max(
                0,
                l.subtotal - l.subtotalAcumuladoNotasCredito,
            );
            const precioEfectivo =
                l.cantidad > 0
                    ? subtotalDisponibleLinea / l.cantidad
                    : l.precioUnitario;
            // Por defecto:
            //  - Devolución: cantidad 0 (cajero indica), precio = efectivo.
            //  - Corrige monto: cantidad = disponible, precio = 0 (cajero indica delta).
            //  - Anulación: cantidad = disponible, precio = efectivo.
            const cantidadDefault =
                modo === MODO_NOTA_CREDITO.Devolucion ? 0 : cantidadDisponible;
            const precioDefault =
                modo === MODO_NOTA_CREDITO.CorrigeMonto ? 0 : precioEfectivo;
            return [{
                productoId: l.productoId!,
                cantidadOriginal: l.cantidad,
                cantidadDisponible,
                cantidad: cantidadDefault,
                precioOriginal: l.precioUnitario,
                subtotalOriginal: l.subtotal,
                subtotalRestadoPrevio: l.subtotalAcumuladoNotasCredito,
                precioUnitario: precioDefault,
                porcentajeIva,
                descripcion: l.descripcion,
                descripcionLinea: l.descripcion,
                codigo: l.codigo,
                reintegrar: true,
            }];
        });
}

// Outer: el Drawer persiste para la animación. Mantine desmonta el contenido al
// cerrar (keepMounted=false), así arranca limpio en cada apertura. `modo` vive
// en el outer y la key del inner incluye el modo: cambiarlo remonta el form con
// los defaults del nuevo modo — sin efecto de reset.
export default function EmitirNotaCreditoDrawer({ opened, onClose, documento }: Props) {
    const loadingRef = useRef(false);
    // NC contra una ND solo reversa monto (la ND nunca movió stock): arranca en
    // Corrige monto (el modo Devolución se oculta en el inner).
    const esOrigenNotaDebito = documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaDebito;
    const [modo, setModo] = useState<ModoNotaCreditoValue>(
        esOrigenNotaDebito ? MODO_NOTA_CREDITO.CorrigeMonto : MODO_NOTA_CREDITO.Devolucion,
    );

    return (
        <Drawer
            opened={opened}
            onClose={() => {
                if (!loadingRef.current) onClose();
            }}
            position="right"
            size="xl"
            title="Emitir nota de crédito"
        >
            <NotaCreditoFormContent
                key={`${documento.id}:${modo}`}
                documento={documento}
                modo={modo}
                onModoChange={setModo}
                onClose={onClose}
                loadingRef={loadingRef}
            />
        </Drawer>
    );
}

// El formulario representa una transacción única con topes cruzados entre líneas, totales y reembolsos.
// react-doctor-disable-next-line react-doctor/no-giant-component
function NotaCreditoFormContent({
    documento,
    modo,
    onModoChange,
    onClose,
    loadingRef,
}: {
    documento: DocumentoVentaDto;
    modo: ModoNotaCreditoValue;
    onModoChange: (modo: ModoNotaCreditoValue) => void;
    onClose: () => void;
    loadingRef: RefObject<boolean>;
}) {
    const { push, refresh } = useRouter();
    const esOrigenNotaDebito = documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaDebito;
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
        lineas: construirLineasNC(documento, modo),
        loading: false,
        errorGlobal: null as string | null,
    }));
    const setRazon = (razon: string) => patchState({ razon });
    const setObservaciones = (observaciones: string) => patchState({ observaciones });
    const setLineas = (action: SetStateAction<LineaSeleccionada[]>) =>
        patchState((state) => ({
            lineas: typeof action === "function" ? action(state.lineas) : action,
        }));
    const setLoading = (loading: boolean) => patchState({ loading });
    const setErrorGlobal = (errorGlobal: string | null) => patchState({ errorGlobal });

    // El outer lee este ref para bloquear el cierre mientras se emite.
    loadingRef.current = loading;

    // NCs ya emitidas sobre este documento — descuentan del cap disponible
    // para no exceder el total origen (regla Hacienda).
    const notasCreditoPrevias = useMemo(
        () =>
            documento.documentosGenerados.filter(
                (d) =>
                    d.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaCredito &&
                    d.estado === ESTADO_DOCUMENTO.EMITIDO,
            ),
        [documento.documentosGenerados],
    );
    const montoRestadoPrevio = useMemo(
        () => notasCreditoPrevias.reduce((acc, n) => acc + n.totalComprobante, 0),
        [notasCreditoPrevias],
    );
    const saldoDisponibleNC = Math.max(0, documento.totalComprobante - montoRestadoPrevio);

    const totales = useMemo(() => {
        let subtotal = 0;
        let impuesto = 0;
        for (const l of lineas) {
            const sub = l.cantidad * l.precioUnitario;
            subtotal += sub;
            impuesto += sub * l.porcentajeIva;
        }
        return { subtotal, impuesto, total: subtotal + impuesto };
    }, [lineas]);

    // Una línea aporta a la NC si:
    //  - Devolución/Anulación: cantidad > 0 (precio = original fijo).
    //  - Corrige monto: precio > 0 (cantidad = original fija).
    const lineasAportan = lineas.filter((l) =>
        modo === MODO_NOTA_CREDITO.CorrigeMonto
            ? l.precioUnitario > 0
            : l.cantidad > 0,
    );

    // Tope por línea en Corrige monto:
    //   min(subtotal disponible de la línea / cantidad,
    //       (saldoDisponibleNC − total aportado por las otras líneas con IVA) / (cantidad * (1+iva)))
    // Así el input no deja teclear más allá del saldo global aún con varias líneas.
    function precioMaxParaLinea(productoId: string): number {
        const l = lineas.find((x) => x.productoId === productoId);
        if (!l || l.cantidad <= 0) return 0;
        const subtotalDisponibleLinea = Math.max(
            0,
            l.subtotalOriginal - l.subtotalRestadoPrevio,
        );
        const precioMaxLinea = subtotalDisponibleLinea / l.cantidad;
        const contribOtros = lineas
            .filter((x) => x.productoId !== productoId)
            .reduce((acc, x) => acc + x.cantidad * x.precioUnitario * (1 + x.porcentajeIva), 0);
        const saldoRestante = Math.max(0, saldoDisponibleNC - contribOtros);
        const precioMaxGlobal = saldoRestante / (l.cantidad * (1 + l.porcentajeIva));
        return Math.min(precioMaxLinea, precioMaxGlobal);
    }

    function actualizarCantidad(productoId: string, cantidad: number) {
        setLineas((prev) =>
            prev.map((l) =>
                l.productoId === productoId
                    ? { ...l, cantidad: Math.min(Math.max(cantidad, 0), l.cantidadDisponible) }
                    : l,
            ),
        );
    }

    function toggleReintegrar(productoId: string, reintegrar: boolean) {
        setLineas((prev) =>
            prev.map((l) => (l.productoId === productoId ? { ...l, reintegrar } : l)),
        );
    }

    function actualizarPrecio(productoId: string, precio: number) {
        const tope = precioMaxParaLinea(productoId);
        setLineas((prev) =>
            prev.map((l) =>
                l.productoId === productoId
                    ? { ...l, precioUnitario: Math.min(Math.max(precio, 0), tope) }
                    : l,
            ),
        );
    }

    async function handleEmitir() {
        setErrorGlobal(null);
        const notificarValidacion = (msg: string) => {
            setErrorGlobal(msg);
            AppNotifier.warning({ message: msg });
        };
        if (modo !== MODO_NOTA_CREDITO.Anulacion && lineasAportan.length === 0) {
            notificarValidacion(
                modo === MODO_NOTA_CREDITO.CorrigeMonto
                    ? "Indica el monto a restar en al menos una línea."
                    : "Selecciona al menos una línea con cantidad mayor a cero.",
            );
            return;
        }

        if (totales.total > saldoDisponibleNC + 0.005) {
            notificarValidacion(
                `El total de esta NC (${formatMonedaPorCodigo(totales.total, documento.monedaCodigo)}) excede el saldo disponible (${formatMonedaPorCodigo(saldoDisponibleNC, documento.monedaCodigo)}).`,
            );
            return;
        }
        if (
            modo === MODO_NOTA_CREDITO.CorrigeMonto
            && totales.total >= saldoDisponibleNC - 0.005
        ) {
            notificarValidacion(
                "Para reversar el total del documento usa modo Anulación. 'Corrige monto' es solo para ajuste parcial de precio.",
            );
            return;
        }
        const lineasPayload =
            modo === MODO_NOTA_CREDITO.Anulacion
                ? []
                : lineasAportan.map((l) => ({
                      productoId: l.productoId,
                      cantidad: l.cantidad,
                      precioUnitario: l.precioUnitario,
                      montoDescuento: 0,
                      descripcion: l.descripcionLinea,
                  }));

        // Productos que el usuario desmarcó para no reintegrar (devuelto
        // dañado, etc.). Solo aplica donde se mueve stock.
        const productosSinReintegro =
            modo === MODO_NOTA_CREDITO.CorrigeMonto
                ? undefined
                : lineas.flatMap((l) => (l.reintegrar ? [] : [l.productoId]));

        setLoading(true);
        const result = await emitirNotaCreditoAction({
            documentoOrigenId: documento.id,
            modo,
            lineas: lineasPayload,
            razon,
            observaciones: observaciones || null,
            productosSinReintegro,
        });
        setLoading(false);

        if (result.errors) {
            const msg = resolveApiErrorMessage(result.errors, {
                fallback: "No fue posible emitir la nota de crédito.",
            });
            setErrorGlobal(msg);
            if (result.status >= 500) {
                AppNotifier.error({ message: msg });
            } else {
                AppNotifier.warning({ message: msg });
            }
            return;
        }

        AppNotifier.success({ message: "Nota de crédito emitida." });

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

                <Alert color="blue" variant="light" title="Saldo disponible para NC">
                    <Stack gap={2}>
                        <Group justify="space-between">
                            <Text size="sm" c="dimmed">
                                Total factura
                            </Text>
                            <Text size="sm">
                                {formatMonedaPorCodigo(
                                    documento.totalComprobante,
                                    documento.monedaCodigo,
                                )}
                            </Text>
                        </Group>
                        <Group justify="space-between">
                            <Text size="sm" c="dimmed">
                                NCs ya emitidas ({notasCreditoPrevias.length})
                            </Text>
                            <Text size="sm">
                                −
                                {formatMonedaPorCodigo(
                                    montoRestadoPrevio,
                                    documento.monedaCodigo,
                                )}
                            </Text>
                        </Group>
                        <Group justify="space-between">
                            <Text fw={700} size="sm">
                                Saldo disponible
                            </Text>
                            <Text fw={800}>
                                {formatMonedaPorCodigo(
                                    saldoDisponibleNC,
                                    documento.monedaCodigo,
                                )}
                            </Text>
                        </Group>
                    </Stack>
                </Alert>

                {saldoDisponibleNC <= 0 && (
                    <Alert color="orange" variant="light">
                        Esta factura ya tiene NCs por el total emitido. No queda saldo
                        disponible para emitir más.
                    </Alert>
                )}

                <Stack gap={4}>
                    <Text size="xs" tt="uppercase" fw={700} c="dimmed">
                        Modo
                    </Text>
                    <SegmentedControl
                        value={String(modo)}
                        onChange={(v) => onModoChange(Number(v) as ModoNotaCreditoValue)}
                        data={[
                            ...(esOrigenNotaDebito
                                ? []
                                : [{ value: String(MODO_NOTA_CREDITO.Devolucion), label: "Devolución" }]),
                            { value: String(MODO_NOTA_CREDITO.CorrigeMonto), label: "Corrige monto" },
                            { value: String(MODO_NOTA_CREDITO.Anulacion), label: "Anulación total" },
                        ]}
                    />
                    <Text size="xs" c="dimmed">
                        {modo === MODO_NOTA_CREDITO.Devolucion &&
                            "Cliente devuelve producto. Indica cuántas unidades vuelven; reversa stock."}
                        {modo === MODO_NOTA_CREDITO.CorrigeMonto &&
                            "Se cobró un monto incorrecto. Indica cuánto restar por línea (no devuelve producto, no toca stock)."}
                        {modo === MODO_NOTA_CREDITO.Anulacion &&
                            "Reversa exacta del origen: cantidades y precios completos, reversa stock."}
                    </Text>
                </Stack>

                {modo === MODO_NOTA_CREDITO.Anulacion &&
                    lineas.some((l) => l.cantidadDisponible > 0) && (
                        <Stack gap={4}>
                            <Text size="xs" tt="uppercase" fw={700} c="dimmed">
                                Inventario a reintegrar
                            </Text>
                            <Text size="xs" c="dimmed">
                                Desmarca lo que no vuelve al stock (por ejemplo, mercancía devuelta
                                dañada). El monto se anula completo igual.
                            </Text>
                            <Stack gap={2} pt={4}>
                                {lineas.flatMap((l) =>
                                    l.cantidadDisponible > 0 ? (
                                        <Checkbox
                                            key={l.productoId}
                                            checked={l.reintegrar}
                                            onChange={(e) =>
                                                toggleReintegrar(
                                                    l.productoId,
                                                    e.currentTarget.checked,
                                                )
                                            }
                                            label={`${l.descripcion} (${l.cantidadDisponible})`}
                                        />
                                    ) : [],
                                )}
                            </Stack>
                        </Stack>
                    )}

                {modo !== MODO_NOTA_CREDITO.Anulacion && (
                    <Stack gap={4}>
                        <Text size="xs" tt="uppercase" fw={700} c="dimmed">
                            Líneas
                        </Text>
                        <Table withColumnBorders striped highlightOnHover>
                            <Table.Thead>
                                <Table.Tr>
                                    <Table.Th>Producto</Table.Th>
                                    <Table.Th ta="right">Cant. disp.</Table.Th>
                                    <Table.Th ta="right">Precio efectivo</Table.Th>
                                    {modo === MODO_NOTA_CREDITO.Devolucion && (
                                        <Table.Th ta="right">A devolver</Table.Th>
                                    )}
                                    {modo === MODO_NOTA_CREDITO.CorrigeMonto && (
                                        <Table.Th ta="right">Monto a restar (sin IVA)</Table.Th>
                                    )}
                                    {modo === MODO_NOTA_CREDITO.Devolucion && (
                                        <Table.Th ta="center">Reintegrar</Table.Th>
                                    )}
                                    <Table.Th ta="right">Subtotal NC</Table.Th>
                                </Table.Tr>
                            </Table.Thead>
                            <Table.Tbody>
                                {lineas.map((l) => {
                                    // Precio efectivo unitario: lo que queda del subtotal
                                    // original tras NCs previas, dividido entre cantidad
                                    // original. Si ya se consumió todo, queda en 0.
                                    const subtotalDisponible = Math.max(
                                        0,
                                        l.subtotalOriginal - l.subtotalRestadoPrevio,
                                    );
                                    const precioEfectivo =
                                        l.cantidadOriginal > 0
                                            ? subtotalDisponible / l.cantidadOriginal
                                            : l.precioOriginal;
                                    // Tope de monto a restar en Corrige monto: 0 = línea
                                    // sin saldo, se oculta el input.
                                    const montoMaxLinea =
                                        precioMaxParaLinea(l.productoId) * l.cantidad;
                                    return (
                                    <Table.Tr key={l.productoId}>
                                        <Table.Td>
                                            <Stack gap={0}>
                                                <Text size="sm" fw={600}>
                                                    {l.descripcion}
                                                </Text>
                                                <Text c="dimmed" size="xs">
                                                    {l.codigo}
                                                    {l.cantidadDisponible !== l.cantidadOriginal && (
                                                        <> · {l.cantidadOriginal - l.cantidadDisponible} ya devueltos</>
                                                    )}
                                                </Text>
                                            </Stack>
                                        </Table.Td>
                                        <Table.Td ta="right">
                                            <Stack gap={0} align="flex-end">
                                                <Text size="sm">{l.cantidadDisponible}</Text>
                                                {l.cantidadDisponible !== l.cantidadOriginal && (
                                                    <Text c="dimmed" size="xs">
                                                        de {l.cantidadOriginal}
                                                    </Text>
                                                )}
                                            </Stack>
                                        </Table.Td>
                                        <Table.Td ta="right">
                                            <Stack gap={0} align="flex-end">
                                                <Text size="sm">
                                                    {formatMonedaPorCodigo(
                                                        precioEfectivo,
                                                        documento.monedaCodigo,
                                                    )}
                                                </Text>
                                                {Math.abs(precioEfectivo - l.precioOriginal) > 0.005 && (
                                                    <Text c="dimmed" size="xs" td="line-through">
                                                        {formatMonedaPorCodigo(
                                                            l.precioOriginal,
                                                            documento.monedaCodigo,
                                                        )}
                                                    </Text>
                                                )}
                                            </Stack>
                                        </Table.Td>
                                        {modo === MODO_NOTA_CREDITO.Devolucion && (
                                            <Table.Td ta="right" w={140}>
                                                {l.cantidadDisponible > 0 ? (
                                                    <NumberInput
                                                        size="xs"
                                                        value={l.cantidad}
                                                        min={0}
                                                        max={l.cantidadDisponible}
                                                        decimalScale={2}
                                                        clampBehavior="strict"
                                                        allowNegative={false}
                                                        allowLeadingZeros={false}
                                                        onChange={(v) =>
                                                            actualizarCantidad(
                                                                l.productoId,
                                                                aNumero(v),
                                                            )
                                                        }
                                                    />
                                                ) : (
                                                    <Text c="dimmed" size="sm">
                                                        -
                                                    </Text>
                                                )}
                                            </Table.Td>
                                        )}
                                        {modo === MODO_NOTA_CREDITO.CorrigeMonto && (
                                            <Table.Td ta="right" w={160}>
                                                {montoMaxLinea > 0 ? (
                                                    <NumberInput
                                                        size="xs"
                                                        value={l.precioUnitario * l.cantidad}
                                                        min={0}
                                                        max={montoMaxLinea}
                                                        decimalScale={2}
                                                        clampBehavior="strict"
                                                        allowNegative={false}
                                                        allowLeadingZeros={false}
                                                        onChange={(v) => {
                                                            const monto = aNumero(v);
                                                            actualizarPrecio(
                                                                l.productoId,
                                                                l.cantidad > 0 ? monto / l.cantidad : 0,
                                                            );
                                                        }}
                                                    />
                                                ) : (
                                                    <Text c="dimmed" size="sm">
                                                        -
                                                    </Text>
                                                )}
                                            </Table.Td>
                                        )}
                                        {modo === MODO_NOTA_CREDITO.Devolucion && (
                                            <Table.Td>
                                                <Group justify="center" gap={0}>
                                                    <Checkbox
                                                        checked={l.reintegrar}
                                                        onChange={(e) =>
                                                            toggleReintegrar(
                                                                l.productoId,
                                                                e.currentTarget.checked,
                                                            )
                                                        }
                                                        disabled={l.cantidad <= 0}
                                                        aria-label="Reintegrar al inventario"
                                                    />
                                                </Group>
                                            </Table.Td>
                                        )}
                                        <Table.Td ta="right">
                                            {formatMonedaPorCodigo(
                                                l.cantidad * l.precioUnitario,
                                                documento.monedaCodigo,
                                            )}
                                        </Table.Td>
                                    </Table.Tr>
                                    );
                                })}
                            </Table.Tbody>
                        </Table>
                        <Stack gap={4} pt="xs">
                            <Group justify="space-between">
                                <Text c="dimmed" size="sm">
                                    Subtotal NC
                                </Text>
                                <Text size="sm">
                                    {formatMonedaPorCodigo(totales.subtotal, documento.monedaCodigo)}
                                </Text>
                            </Group>
                            <Group justify="space-between">
                                <Text c="dimmed" size="sm">
                                    IVA NC
                                </Text>
                                <Text size="sm">
                                    {formatMonedaPorCodigo(totales.impuesto, documento.monedaCodigo)}
                                </Text>
                            </Group>
                            <Group justify="space-between">
                                <Text fw={700}>Total NC</Text>
                                <Text fw={800}>
                                    {formatMonedaPorCodigo(totales.total, documento.monedaCodigo)}
                                </Text>
                            </Group>
                        </Stack>
                    </Stack>
                )}

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
                    <Button
                        loading={loading}
                        onClick={handleEmitir}
                        disabled={saldoDisponibleNC <= 0}
                    >
                        Emitir nota
                    </Button>
                </Group>
        </Stack>
    );
}
