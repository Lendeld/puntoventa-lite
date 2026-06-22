"use client";

import { useApartadoAcciones } from "@lib/hooks/ventas/useApartadoAcciones";
import { useAbonoCredito } from "@lib/hooks/ventas/useAbonoCredito";
import { useMediosPagoActivosQuery } from "@lib/hooks/useMediosPagoActivosQuery";
import { MenuItem } from "@components/ui/MenuItem";
import EmitirNotaCreditoDrawer from "@pages/emision/ventas/[id]/EmitirNotaCreditoDrawer";
import EmitirNotaDebitoDrawer from "@pages/emision/ventas/[id]/EmitirNotaDebitoDrawer";
import { AnularAbonoModal } from "@pages/emision/ventas/[id]/AnularAbonoModal";
import { AbonoResultadoModal } from "@pages/emision/ventas/[id]/AbonoResultadoModal";
import { useImprimirTicketAhora } from "@lib/printing/imprimir-ticket";
import { getAbonoPdfUrl, getVentaPdfUrl } from "@lib/printing/venta-printing";
import type {
    DocumentoVentaDto,
    DocumentoVentaLineaDto,
    DocumentoVentaPagoDto,
    DocumentoVentaRelacionadoDto,
} from "@lib/types/ventas.types";
import { TIPO_DOCUMENTO_VENTA } from "@lib/constants/ventas.constants";
import { formatDate } from "@lib/utils/date.utils";
import { ESTADO_DOCUMENTO, formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { redondearMoneda } from "@lib/utils/number.utils";
import {
    ActionIcon,
    Alert,
    Badge,
    Button,
    Card,
    Center,
    Divider,
    FloatingIndicator,
    Grid,
    Group,
    Menu,
    Modal,
    NumberInput,
    Select,
    Stack,
    Table,
    Tabs,
    Text,
    Textarea,
    ThemeIcon,
    Title,
} from "@mantine/core";
import { DateTimePicker } from "@mantine/dates";
import {
    IconArrowLeft,
    IconBan,
    IconCalendarPlus,
    IconCash,
    IconCheck,
    IconChevronDown,
    IconDotsVertical,
    IconExternalLink,
    IconHistory,
    IconLink,
    IconReceipt2,
    IconPrinter,
    IconScale,
    IconTable,
} from "@tabler/icons-react";
import { HistoriaEventosVenta } from "@pages/emision/ventas/[id]/HistoriaEventosVenta";
import dayjs from "dayjs";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { parseAsStringLiteral, useQueryState } from "nuqs";
import type { ReactNode } from "react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useReturnUrl } from "@lib/hooks/useReturnUrl";

interface ApartadoPermisos {
    abonar: boolean;
    extender: boolean;
    convertir: boolean;
    cancelar: boolean;
    emitirNotaCredito?: boolean;
    emitirNotaDebito?: boolean;
    abonarCredito?: boolean;
    anularAbono?: boolean;
}

interface Props {
    documento: DocumentoVentaDto;
    permisos?: ApartadoPermisos;
}

const PERMISOS_DEFAULT: ApartadoPermisos = {
    abonar: false,
    extender: false,
    convertir: false,
    cancelar: false,
    emitirNotaCredito: false,
    emitirNotaDebito: false,
    abonarCredito: false,
    anularAbono: false,
};

const TABS_VALIDAS = ["detalle", "historia"] as const;
type TabId = (typeof TABS_VALIDAS)[number];

function formatFecha(value: string) {
    return formatDate(value, "datetime");
}

function formatCantidad(value: number) {
    return value.toLocaleString("en-US", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
    });
}

function LabelValue({ label, value }: { label: string; value: ReactNode }) {
    return (
        <Stack gap={2}>
            <Text c="dimmed" size="xs" tt="uppercase" fw={700}>
                {label}
            </Text>
            <Text size="sm" fw={500}>
                {value || "No indicado"}
            </Text>
        </Stack>
    );
}

function DetailCard({
    title,
    description,
    icon,
    children,
    className = "",
}: {
    title: string;
    description?: string;
    icon?: ReactNode;
    children: ReactNode;
    className?: string;
}) {
    return (
        <Card
            radius="lg"
            p="lg"
            className={`${className}`}
        >
            <Stack gap="md">
                <Group justify="space-between" align="flex-start">
                    <Group gap="xs">
                        {icon && (
                            <ThemeIcon variant="light" color="accentPV">
                                {icon}
                            </ThemeIcon>
                        )}
                        <Stack gap={2}>
                            <Text fw={700}>{title}</Text>
                            {description && (
                                <Text size="sm" c="dimmed">
                                    {description}
                                </Text>
                            )}
                        </Stack>
                    </Group>
                </Group>
                {children}
            </Stack>
        </Card>
    );
}

function DocumentoRelacionadoLink({
    documento,
}: {
    documento: DocumentoVentaRelacionadoDto;
}) {
    return (
        <Card
            radius="md"
            p="md"
            className="bg-theme-surface border border-theme"
        >
            <Group justify="space-between" align="flex-start" gap="md">
                <Stack gap={4}>
                    <Group gap="xs">
                        <Badge
                            color={documento.tipoDocumentoColor || "gray"}
                            variant="light"
                        >
                            {documento.tipoDocumentoDetalle}
                        </Badge>
                        <Badge
                            color={documento.estadoColor || "gray"}
                            variant="light"
                        >
                            {documento.estadoDetalle}
                        </Badge>
                    </Group>
                    <Text fw={800}>
                        {documento.consecutivo ?? "Sin consecutivo"}
                    </Text>
                    <Text c="dimmed" size="xs">
                        {formatFecha(documento.fechaDocumento)}
                    </Text>
                </Stack>
                <Stack align="flex-end" gap={8}>
                    <Text fw={700}>
                        {formatMonedaPorCodigo(
                            documento.totalComprobante,
                            documento.monedaCodigo,
                        )}
                    </Text>
                    <Button
                        component={Link}
                        href={`/emision/ventas/${documento.id}`}
                        size="xs"
                        variant="light"
                        rightSection={<IconExternalLink size={14} />}
                    >
                        Ver detalle
                    </Button>
                </Stack>
            </Group>
        </Card>
    );
}

function LineasTable({
    lineas,
    monedaCodigo,
}: {
    lineas: DocumentoVentaLineaDto[];
    monedaCodigo: string;
}) {
    return (
        <div className="overflow-x-auto">
            <Table striped highlightOnHover verticalSpacing="sm">
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Codigo</Table.Th>
                        <Table.Th>Descripcion</Table.Th>
                        <Table.Th ta="right">Cantidad</Table.Th>
                        <Table.Th ta="right">Precio</Table.Th>
                        <Table.Th ta="right">Descuento</Table.Th>
                        <Table.Th ta="right">Impuesto</Table.Th>
                        <Table.Th ta="right">Total</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {lineas.map((linea) => (
                        <Table.Tr key={linea.id}>
                            <Table.Td>{linea.codigo}</Table.Td>
                            <Table.Td>
                                <Stack gap={2}>
                                    <Text size="sm" fw={600}>
                                        {linea.descripcion}
                                    </Text>
                                    <Text c="dimmed" size="xs">
                                        {linea.unidadMedidaCodigo}
                                    </Text>
                                </Stack>
                            </Table.Td>
                            <Table.Td ta="right">
                                {formatCantidad(linea.cantidad)}
                            </Table.Td>
                            <Table.Td ta="right">
                                {formatMonedaPorCodigo(
                                    linea.precioUnitario,
                                    monedaCodigo,
                                )}
                            </Table.Td>
                            <Table.Td ta="right">
                                {formatMonedaPorCodigo(
                                    linea.montoDescuento,
                                    monedaCodigo,
                                )}
                            </Table.Td>
                            <Table.Td ta="right">
                                {formatMonedaPorCodigo(
                                    linea.montoImpuesto,
                                    monedaCodigo,
                                )}
                            </Table.Td>
                            <Table.Td ta="right">
                                <Text fw={700}>
                                    {formatMonedaPorCodigo(
                                        linea.totalLinea,
                                        monedaCodigo,
                                    )}
                                </Text>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </div>
    );
}

function renderAccionesAbonoMenuTarget() {
    return (
        <ActionIcon
            aria-label="Acciones"
            title="Acciones"
            variant="subtle"
            color="gray"
            size="sm"
        >
            <IconDotsVertical size={18} />
        </ActionIcon>
    );
}

function PagosTable({
    pagos,
    monedaCodigo,
    documentoId,
    puedeEmitirRecibo,
    puedeAnularAbono,
    esFacturaCredito,
    onAnularPago,
}: {
    pagos: DocumentoVentaPagoDto[];
    monedaCodigo: string;
    documentoId: string;
    puedeEmitirRecibo: boolean;
    puedeAnularAbono: boolean;
    esFacturaCredito: boolean;
    onAnularPago: (pagoId: string) => void;
}) {
    const imprimirTicket = useImprimirTicketAhora();
    if (pagos.length === 0) {
        return (
            <Text c="dimmed" size="sm">
                Este documento no tiene pagos registrados.
            </Text>
        );
    }

    return (
        <div className="overflow-x-auto">
            <Table striped highlightOnHover verticalSpacing="sm">
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Abono</Table.Th>
                        <Table.Th ta="right">Monto</Table.Th>
                        <Table.Th>Fechas</Table.Th>
                        <Table.Th>Referencia</Table.Th>
                        <Table.Th>Estado</Table.Th>
                        <Table.Th>Usuarios</Table.Th>
                        <Table.Th ta="center">Acciones</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {pagos.map((pago) => (
                        <Table.Tr key={pago.id} opacity={pago.anulado ? 0.6 : 1}>
                            <Table.Td>
                                <Stack gap={2}>
                                    <Text size="sm">{pago.medioPagoDetalleSnapshot}</Text>
                                    {esFacturaCredito && pago.numeroAbono > 0 && (
                                        <Text size="xs" c="dimmed">
                                            Abono #{pago.numeroAbono}
                                        </Text>
                                    )}
                                    <Text size="xs" c="dimmed">
                                        {pago.monedaCodigo}
                                    </Text>
                                </Stack>
                            </Table.Td>
                            <Table.Td ta="center">
                                <Stack gap={2} align="flex-end">
                                    <Text fw={700}>
                                        {formatMonedaPorCodigo(
                                            pago.montoAplicadoDocumento,
                                            monedaCodigo,
                                        )}
                                    </Text>
                                    <Text size="xs" c="dimmed">
                                        Entregado: {formatMonedaPorCodigo(
                                            pago.montoEntregado,
                                            pago.monedaCodigo,
                                        )}
                                    </Text>
                                    {pago.montoVueltoDocumento > 0 && (
                                        <Text size="xs" c="dimmed">
                                            Vuelto: {formatMonedaPorCodigo(
                                                pago.montoVueltoDocumento,
                                                monedaCodigo,
                                            )}
                                        </Text>
                                    )}
                                </Stack>
                            </Table.Td>
                            <Table.Td>
                                <Stack gap={2}>
                                    <Text size="xs" c="dimmed">
                                        Informativa
                                    </Text>
                                    <Text size="sm">{formatFecha(pago.fechaPago)}</Text>
                                    <Text size="xs" c="dimmed">
                                        Registro real
                                    </Text>
                                    <Text size="sm">{formatFecha(pago.fechaRegistroUtc)}</Text>
                                    {pago.fechaAnulacionUtc && (
                                        <>
                                            <Text size="xs" c="dimmed">
                                                Anulación
                                            </Text>
                                            <Text size="sm">
                                                {formatFecha(pago.fechaAnulacionUtc)}
                                            </Text>
                                        </>
                                    )}
                                </Stack>
                            </Table.Td>
                            <Table.Td>
                                <Stack gap={2}>
                                    <Text size="sm">{pago.referencia ?? "—"}</Text>
                                    {pago.observacion && (
                                        <Text size="xs" c="dimmed">
                                            {pago.observacion}
                                        </Text>
                                    )}
                                </Stack>
                            </Table.Td>
                            <Table.Td>
                                <Stack gap={4}>
                                    <Badge
                                        color={pago.anulado ? "gray" : "green"}
                                        variant="light"
                                        size="sm"
                                        w="fit-content"
                                    >
                                        {pago.anulado ? "Anulado" : "Activo"}
                                    </Badge>
                                    {pago.motivoAnulacion && (
                                        <Text size="xs" c="dimmed">
                                            Motivo: {pago.motivoAnulacion}
                                        </Text>
                                    )}
                                </Stack>
                            </Table.Td>
                            <Table.Td>
                                <Stack gap={2}>
                                    <Text size="sm">
                                        Registra: {pago.usuarioRegistroNombre ?? "-"}
                                    </Text>
                                    {pago.usuarioAnulaNombre && (
                                        <Text size="xs" c="dimmed">
                                            Anula: {pago.usuarioAnulaNombre}
                                        </Text>
                                    )}
                                </Stack>
                            </Table.Td>
                            <Table.Td ta="center">
                                <Center w="100%">
                                    {pago.anulado ? (
                                        puedeEmitirRecibo ? (
                                            <Menu position="bottom-end" withinPortal>
                                                <Menu.Target>
                                                    {renderAccionesAbonoMenuTarget()}
                                                </Menu.Target>
                                                <Menu.Dropdown>
                                                    <Menu.Item
                                                        leftSection={<IconPrinter size={14} />}
                                                        onClick={() => imprimirTicket(documentoId, pago.id)}
                                                    >
                                                        Imprimir ticket
                                                    </Menu.Item>
                                                    <Menu.Item
                                                        component="a"
                                                        href={getAbonoPdfUrl(documentoId, pago.id)}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        leftSection={<IconExternalLink size={14} />}
                                                    >
                                                        Abrir PDF
                                                    </Menu.Item>
                                                </Menu.Dropdown>
                                            </Menu>
                                        ) : (
                                            <Text c="dimmed" size="xs">
                                                Anulado
                                            </Text>
                                        )
                                    ) : puedeEmitirRecibo || (puedeAnularAbono && esFacturaCredito) ? (
                                        <Menu position="bottom-end" withinPortal>
                                            <Menu.Target>
                                                {renderAccionesAbonoMenuTarget()}
                                            </Menu.Target>
                                            <Menu.Dropdown>
                                                {puedeEmitirRecibo && (
                                                    <>
                                                        <Menu.Item
                                                            leftSection={<IconPrinter size={14} />}
                                                            onClick={() => imprimirTicket(documentoId, pago.id)}
                                                        >
                                                            Imprimir ticket
                                                        </Menu.Item>
                                                        <Menu.Item
                                                            component="a"
                                                            href={getAbonoPdfUrl(documentoId, pago.id)}
                                                            target="_blank"
                                                            rel="noopener noreferrer"
                                                            leftSection={<IconExternalLink size={14} />}
                                                        >
                                                            Abrir PDF
                                                        </Menu.Item>
                                                    </>
                                                )}
                                                {puedeEmitirRecibo &&
                                                    puedeAnularAbono &&
                                                    esFacturaCredito && <Menu.Divider />}
                                                {puedeAnularAbono && esFacturaCredito && (
                                                    <MenuItem
                                                        variant="danger"
                                                        leftSection={<IconBan size={14} />}
                                                        onClick={() => onAnularPago(pago.id)}
                                                    >
                                                        Anular abono
                                                    </MenuItem>
                                                )}
                                            </Menu.Dropdown>
                                        </Menu>
                                    ) : (
                                        <Text c="dimmed" size="xs">
                                            Sin recibo
                                        </Text>
                                    )}
                                </Center>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </div>
    );
}

// Las tablas y tarjetas reutilizables ya están extraídas; este componente conserva la orquestación del detalle.
// react-doctor-disable-next-line react-doctor/no-giant-component
export default function DetalleVentaPageSection({
    documento,
    permisos = PERMISOS_DEFAULT,
}: Props) {
    const router = useRouter();
    const imprimirTicket = useImprimirTicketAhora();
    const pdfUrl = getVentaPdfUrl(documento.id);
    const esProforma = documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Proforma;
    const esFacturaCredito =
        documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Factura &&
        documento.esCredito;
    const esApartadoOperable =
        documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Apartado &&
        (documento.estado === ESTADO_DOCUMENTO.RESERVADO ||
            documento.estado === ESTADO_DOCUMENTO.VENCIDO);
    const esFacturaEmitida =
        (documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Factura ||
            documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaDebito) &&
        documento.estado === ESTADO_DOCUMENTO.EMITIDO;
    const cajaEstado = { puedeOperar: true, razonBloqueo: null as string | null };
    const montoNotasCredito = useMemo(
        () =>
            documento.documentosGenerados
                .filter(
                    (d) =>
                        d.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaCredito &&
                        d.estado === ESTADO_DOCUMENTO.EMITIDO,
                )
                .reduce((acc, n) => acc + n.totalComprobante, 0),
        [documento.documentosGenerados],
    );
    // ND emitidas suman cargos adicionales sobre la factura (lo que el cliente
    // debe sube). Se usa el SALDO vigente de cada ND (total − NCs aplicadas a
    // esa ND): una ND ya reversada por su propia NC no infla el cargo ni impide
    // que la factura salga anulada. El neto efectivo = total + ND − NC.
    const montoNotasDebito = useMemo(
        () =>
            documento.documentosGenerados
                .filter(
                    (d) =>
                        d.tipoDocumento === TIPO_DOCUMENTO_VENTA.NotaDebito &&
                        d.estado === ESTADO_DOCUMENTO.EMITIDO,
                )
                .reduce(
                    (acc, n) =>
                        acc +
                        Math.max(
                            0,
                            n.totalComprobante - n.montoNotasCreditoAplicadas,
                        ),
                    0,
                ),
        [documento.documentosGenerados],
    );
    const totalConCargos = documento.totalComprobante + montoNotasDebito;
    const anuladaPorNC = montoNotasCredito >= totalConCargos - 0.005;
    const tieneAbonosActivos = documento.pagos.some((pago) => !pago.anulado);
    // Pagado/Saldo solo aporta en pagos diferidos (crédito o saldo pendiente).
    // En contado pagado al instante son redundantes. Se compara a escala de
    // pago (2 dec): el residuo sub-céntimo (total 5 dec vs pago 2 dec) no es saldo.
    const mostrarPagoSaldo = documento.esCredito || redondearMoneda(documento.saldoPendiente) > 0;
    const bloqueaNotaCreditoPorAbonos = esFacturaCredito && tieneAbonosActivos;
    const puedeEmitirNotaCredito =
        esFacturaEmitida &&
        !anuladaPorNC &&
        !bloqueaNotaCreditoPorAbonos &&
        (permisos.emitirNotaCredito ?? false) &&
        cajaEstado.puedeOperar;
    const [notaCreditoOpen, setNotaCreditoOpen] = useState(false);
    // ND solo contra factura de contado (no contra otra ND ni NC): es un cargo
    // adicional. En crédito se bloquea para no mezclar cargos con el saldo cobrable.
    const esFacturaParaNotaDebito =
        documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Factura &&
        documento.estado === ESTADO_DOCUMENTO.EMITIDO &&
        !documento.esCredito;
    const puedeEmitirNotaDebito =
        esFacturaParaNotaDebito &&
        !anuladaPorNC &&
        (permisos.emitirNotaDebito ?? false) &&
        cajaEstado.puedeOperar;
    const [notaDebitoOpen, setNotaDebitoOpen] = useState(false);
    const volverHref = useReturnUrl("ventas", "/emision/ventas");
    const [anularAbonoModalPagoId, setAnularAbonoModalPagoId] = useState<string | null>(null);

    const {
        loadingAction,
        abonoOpen,
        abrirAbono,
        closeAbonoModal,
        montoAbono,
        setMontoAbono,
        medioPagoCodigo,
        setMedioPagoCodigo,
        referenciaPago,
        setReferenciaPago,
        observacionPago,
        setObservacionPago,
        fechaPago,
        setFechaPago,
        abonoErrors,
        limpiarAbonoError,
        abonoError,
        handleAbonar,
        extensionOpen,
        setExtensionOpen,
        fechaVencimiento,
        setFechaVencimiento,
        handleExtender,
        handleCancelarApartado,
        handleConvertirApartado,
    } = useApartadoAcciones(documento, router);

    const {
        loadingAbono,
        abonoCreditoOpen,
        abrirAbonoCredito,
        cerrarAbonoCredito,
        montoAbono: montoAbonoCredito,
        setMontoAbono: setMontoAbonoCredito,
        medioPagoCodigo: medioPagoCodigoCredito,
        setMedioPagoCodigo: setMedioPagoCodigoCredito,
        referenciaPago: referenciaPagoCredito,
        setReferenciaPago: setReferenciaPagoCredito,
        observacionPago: observacionPagoCredito,
        setObservacionPago: setObservacionPagoCredito,
        fechaPago: fechaPagoCredito,
        setFechaPago: setFechaPagoCredito,
        abonoCreditoErrors,
        limpiarAbonoCreditoError,
        abonoCreditoError,
        handleAbonarCredito,
        abonoResultadoPagoId,
        cerrarAbonoResultado,
    } = useAbonoCredito(documento, router);
    const puedeAbonarFacturaCredito =
        esFacturaCredito &&
        documento.estado === ESTADO_DOCUMENTO.EMITIDO &&
        redondearMoneda(documento.saldoPendiente) > 0 &&
        (permisos.abonarCredito ?? false) &&
        cajaEstado.puedeOperar;
    const { data: mediosPago = [] } = useMediosPagoActivosQuery();
    // Tab sincronizado en la URL via nuqs. clearOnDefault (default en nuqs v2)
    // quita ?tab cuando vuelve a "detalle".
    const [tabActiva, setTabActiva] = useQueryState(
        "tab",
        parseAsStringLiteral(TABS_VALIDAS)
            .withDefault("detalle")
            .withOptions({ scroll: false }),
    );
    const [tabsListNode, setTabsListNode] = useState<HTMLDivElement | null>(null);
    const tabRefs = useRef<Record<string, HTMLButtonElement | null>>({});
    const [tabIndicatorTarget, setTabIndicatorTarget] =
        useState<HTMLButtonElement | null>(null);
    const setTabRef = useCallback(
        (value: string) => (node: HTMLButtonElement | null) => {
            tabRefs.current[value] = node;
        },
        [],
    );
    const refDetalle = useMemo(() => setTabRef("detalle"), [setTabRef]);
    const refHistoria = useMemo(() => setTabRef("historia"), [setTabRef]);
    useEffect(() => {
        setTabIndicatorTarget(tabRefs.current[tabActiva] ?? null);
    }, [tabActiva]);
    const diasVencimiento = useMemo(() => {
        if (!documento.fechaVencimiento) return null;
        return dayjs(documento.fechaVencimiento)
            .startOf("day")
            .diff(dayjs().startOf("day"), "day");
    }, [documento.fechaVencimiento]);


    return (
        <Stack gap="lg">
            <EmitirNotaCreditoDrawer
                opened={notaCreditoOpen}
                onClose={() => setNotaCreditoOpen(false)}
                documento={documento}
            />
            <EmitirNotaDebitoDrawer
                opened={notaDebitoOpen}
                onClose={() => setNotaDebitoOpen(false)}
                documento={documento}
            />
            {anularAbonoModalPagoId && (
                <AnularAbonoModal
                    documentoId={documento.id}
                    pagoId={anularAbonoModalPagoId}
                    montoAplicado={
                        documento.pagos.find((pago) => pago.id === anularAbonoModalPagoId)
                            ?.montoAplicadoDocumento ?? 0
                    }
                    monedaCodigo={documento.monedaCodigo}
                    consecutivo={documento.consecutivo}
                    onClose={() => setAnularAbonoModalPagoId(null)}
                />
            )}
            {abonoResultadoPagoId && (
                <AbonoResultadoModal
                    opened
                    facturaId={documento.id}
                    pagoId={abonoResultadoPagoId}
                    consecutivo={documento.consecutivo}
                    montoAbono={
                        typeof montoAbonoCredito === "number"
                            ? montoAbonoCredito
                            : null
                    }
                    monedaCodigo={documento.monedaCodigo}
                    onClose={cerrarAbonoResultado}
                />
            )}
            <Modal
                opened={abonoCreditoOpen}
                onClose={cerrarAbonoCredito}
                title="Registrar abono a factura"
                centered
            >
                <Stack gap="sm">
                    {abonoCreditoError && (
                        <Alert color="orange" variant="light">
                            {abonoCreditoError}
                        </Alert>
                    )}
                    <DateTimePicker
                        label="Fecha informativa del abono"
                        value={fechaPagoCredito ? dayjs(fechaPagoCredito).toDate() : null}
                        onChange={(value) => {
                            setFechaPagoCredito(value ? dayjs(value).toISOString() : null);
                            limpiarAbonoCreditoError("fechaPago");
                        }}
                        error={abonoCreditoErrors.fechaPago}
                        valueFormat="DD/MM/YYYY hh:mm A"
                        locale="es"
                        timePickerProps={{ format: "12h" }}
                        maxDate={new Date()}
                    />
                    <Select
                        label="Medio de pago"
                        data={mediosPago.map((medio) => ({
                            value: medio.codigo,
                            label: medio.detalle,
                        }))}
                        value={medioPagoCodigoCredito || null}
                        onChange={(value) => {
                            setMedioPagoCodigoCredito(value ?? "");
                            limpiarAbonoCreditoError("medioPagoCodigo");
                        }}
                        error={abonoCreditoErrors.medioPagoCodigo}
                        required
                    />
                    <NumberInput
                        label="Monto"
                        value={montoAbonoCredito}
                        min={0.01}
                        max={documento.saldoPendiente}
                        decimalScale={2}
                        onChange={(value) => {
                            setMontoAbonoCredito(
                                typeof value === "number" ? value : "",
                            );
                            limpiarAbonoCreditoError("montoAbono");
                        }}
                        error={abonoCreditoErrors.montoAbono}
                        required
                    />
                    <Textarea
                        label="Referencia"
                        value={referenciaPagoCredito}
                        onChange={(event) =>
                            setReferenciaPagoCredito(event.currentTarget.value)
                        }
                        rows={2}
                    />
                    <Textarea
                        label="Observación"
                        value={observacionPagoCredito}
                        onChange={(event) =>
                            setObservacionPagoCredito(event.currentTarget.value)
                        }
                        rows={2}
                    />
                    <Group justify="flex-end">
                        <Button variant="light" onClick={cerrarAbonoCredito}>
                            Cancelar
                        </Button>
                        <Button loading={loadingAbono} onClick={handleAbonarCredito}>
                            Registrar
                        </Button>
                    </Group>
                </Stack>
            </Modal>
            <Modal
                opened={abonoOpen}
                onClose={closeAbonoModal}
                title="Registrar abono"
                centered
            >
                <Stack gap="sm">
                    {abonoError && (
                        <Alert color="red" variant="light">
                            {abonoError}
                        </Alert>
                    )}
                    <DateTimePicker
                        label="Fecha de pago"
                        value={fechaPago}
                        onChange={(value) => {
                            setFechaPago(value ? dayjs(value).toISOString() : null);
                            limpiarAbonoError("fechaPago");
                        }}
                        error={abonoErrors.fechaPago}
                        valueFormat="DD/MM/YYYY hh:mm A"
                        locale="es"
                        timePickerProps={{ format: "12h" }}
                    />
                    <Select
                        label="Medio de pago"
                        data={mediosPago.map((medio) => ({
                            value: medio.codigo,
                            label: medio.detalle,
                        }))}
                        value={medioPagoCodigo || null}
                        onChange={(value) => {
                            setMedioPagoCodigo(value ?? "");
                            limpiarAbonoError("medioPagoCodigo");
                        }}
                        error={abonoErrors.medioPagoCodigo}
                        required
                    />
                    <NumberInput
                        label="Monto"
                        value={montoAbono}
                        min={0.01}
                        max={documento.saldoPendiente}
                        decimalScale={2}
                        onChange={(value) => {
                            setMontoAbono(
                                typeof value === "number" ? value : "",
                            );
                            limpiarAbonoError("montoAbono");
                        }}
                        error={abonoErrors.montoAbono}
                        required
                    />
                    <Textarea
                        label="Referencia"
                        value={referenciaPago}
                        onChange={(event) =>
                            setReferenciaPago(event.currentTarget.value)
                        }
                    />
                    <Textarea
                        label="Observación"
                        value={observacionPago}
                        onChange={(event) =>
                            setObservacionPago(event.currentTarget.value)
                        }
                    />
                    <Group justify="flex-end">
                        <Button variant="light" onClick={closeAbonoModal}>
                            Cancelar
                        </Button>
                        <Button loading={loadingAction} onClick={handleAbonar}>
                            Registrar
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            <Modal
                opened={extensionOpen}
                onClose={() => setExtensionOpen(false)}
                title="Extender vencimiento"
                centered
            >
                <Stack gap="sm">
                    <DateTimePicker
                        label="Nueva fecha"
                        value={fechaVencimiento}
                        onChange={(value) => setFechaVencimiento(value ? dayjs(value).toISOString() : null)}
                        minDate={dayjs(documento.fechaDocumento).toDate()}
                        valueFormat="DD/MM/YYYY hh:mm A"
                        locale="es"
                        timePickerProps={{ format: "12h" }}
                    />
                    <Group justify="flex-end">
                        <Button
                            variant="light"
                            onClick={() => setExtensionOpen(false)}
                        >
                            Cancelar
                        </Button>
                        <Button
                            loading={loadingAction}
                            onClick={handleExtender}
                        >
                            Guardar
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            <Group justify="space-between" align="flex-start" gap="md">
                <Stack gap={8}>
                    <Button
                        component={Link}
                        href={volverHref}
                        variant="light"
                        size="xs"
                        leftSection={<IconArrowLeft size={14} />}
                        w="fit-content"
                    >
                        Volver a ventas
                    </Button>
                    <Group gap="xs">
                        <Badge
                            color={documento.tipoDocumentoColor || "gray"}
                            variant="light"
                        >
                            {documento.tipoDocumentoDetalle}
                        </Badge>
                        <Badge
                            color={documento.estadoColor || "gray"}
                            variant="light"
                        >
                            {documento.estadoDetalle}
                        </Badge>
                    </Group>
                    <Title order={1} size="2rem">
                        {documento.consecutivo ?? "Documento sin consecutivo"}
                    </Title>
                    {esApartadoOperable && diasVencimiento != null && (
                        <Text
                            c={
                                diasVencimiento < 0
                                    ? "red"
                                    : diasVencimiento <= 3
                                      ? "orange"
                                      : "dimmed"
                            }
                            size="sm"
                        >
                            {diasVencimiento < 0
                                ? `Vencido hace ${Math.abs(diasVencimiento)} días`
                                : `Vence en ${diasVencimiento} días`}
                        </Text>
                    )}
                </Stack>

                <Stack gap="xs" align="flex-end">
                    <Group gap="xs" justify="flex-end">
                        {puedeEmitirNotaCredito && (
                            <Button
                                color="red"
                                variant="light"
                                leftSection={<IconReceipt2 size={16} />}
                                onClick={() => setNotaCreditoOpen(true)}
                            >
                                Emitir NC
                            </Button>
                        )}
                        {puedeEmitirNotaDebito && (
                            <Button
                                color="orange"
                                variant="light"
                                leftSection={<IconReceipt2 size={16} />}
                                onClick={() => setNotaDebitoOpen(true)}
                            >
                                Emitir ND
                            </Button>
                        )}
                        {esApartadoOperable &&
                            documento.saldoPendiente > 0 &&
                            permisos.abonar && (
                                <Button
                                    color="orange"
                                    leftSection={<IconCash size={16} />}
                                    onClick={abrirAbono}
                                >
                                    Abonar
                                </Button>
                            )}
                        {puedeAbonarFacturaCredito && (
                            <Button
                                color="orange"
                                leftSection={<IconCash size={16} />}
                                onClick={abrirAbonoCredito}
                            >
                                Abonar
                            </Button>
                        )}
                        <Menu position="bottom-end" withinPortal>
                            <Menu.Target>
                                <Button
                                    variant="light"
                                    rightSection={<IconChevronDown size={16} />}
                                >
                                    Acciones
                                </Button>
                            </Menu.Target>
                            <Menu.Dropdown>
                                <Menu.Item
                                    leftSection={<IconPrinter size={14} />}
                                    onClick={() => imprimirTicket(documento.id)}
                                >
                                    Imprimir ticket
                                </Menu.Item>
                                <Menu.Item
                                    component="a"
                                    href={pdfUrl}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    leftSection={<IconExternalLink size={14} />}
                                >
                                    Ver PDF
                                </Menu.Item>
                                {esApartadoOperable &&
                                    (permisos.abonar ||
                                        permisos.extender ||
                                        permisos.convertir ||
                                        permisos.cancelar) && (
                                        <>
                                            <Menu.Divider />
                                            {permisos.abonar &&
                                                documento.saldoPendiente > 0 && (
                                                    <Menu.Item
                                                        leftSection={
                                                            <IconCash size={14} />
                                                        }
                                                        onClick={abrirAbono}
                                                        disabled={loadingAction}
                                                    >
                                                        Abonar
                                                    </Menu.Item>
                                                )}
                                            {permisos.extender && (
                                                <Menu.Item
                                                    leftSection={
                                                        <IconCalendarPlus size={14} />
                                                    }
                                                    onClick={() => setExtensionOpen(true)}
                                                >
                                                    Extender vencimiento
                                                </Menu.Item>
                                            )}
                                            {permisos.convertir && (
                                                <Menu.Item
                                                    color="green"
                                                    leftSection={<IconCheck size={14} />}
                                                    onClick={handleConvertirApartado}
                                                    disabled={
                                                        documento.saldoPendiente > 0 || loadingAction
                                                    }
                                                >
                                                    Convertir a factura
                                                </Menu.Item>
                                            )}
                                            {permisos.cancelar && (
                                                <Menu.Item
                                                    color="red"
                                                    leftSection={<IconBan size={14} />}
                                                    onClick={handleCancelarApartado}
                                                    disabled={loadingAction}
                                                >
                                                    Cancelar apartado
                                                </Menu.Item>
                                            )}
                                        </>
                                    )}
                            </Menu.Dropdown>
                        </Menu>
                    </Group>
                    {esFacturaEmitida &&
                        (permisos.emitirNotaCredito ?? false) &&
                        !cajaEstado.puedeOperar && (
                            <Text c="dimmed" size="xs" maw={280} ta="right">
                                {cajaEstado.razonBloqueo ??
                                    "La nota de crédito no está disponible para este documento."}
                            </Text>
                        )}
                    {esFacturaCredito && (
                        <Group gap="xs" justify="flex-end" maw={420}>
                            {bloqueaNotaCreditoPorAbonos &&
                                (permisos.emitirNotaCredito ?? false) && (
                                    <Badge
                                        color="yellow"
                                        variant="light"
                                        title="No puedes emitir una nota de crédito mientras la factura tenga abonos activos. Anúlalos primero."
                                    >
                                        NC bloqueada: abonos activos
                                    </Badge>
                                )}
                            {(permisos.emitirNotaDebito ?? false) && (
                                <Badge
                                    color="yellow"
                                    variant="light"
                                    title="La nota de débito no aplica a facturas a crédito."
                                >
                                    ND no aplica a crédito
                                </Badge>
                            )}
                        </Group>
                    )}
                </Stack>
            </Group>

            <Tabs
                value={tabActiva}
                onChange={(value) =>
                    setTabActiva(
                        (TABS_VALIDAS as readonly string[]).includes(value ?? "")
                            ? (value as TabId)
                            : "detalle",
                    )
                }
                variant="none"
            >
                <Tabs.List
                    ref={setTabsListNode}
                    className="relative gap-1 border-b border-theme"
                >
                    <Tabs.Tab
                        value="detalle"
                        ref={refDetalle}
                        leftSection={<IconReceipt2 size={14} />}
                        className="relative z-10 px-4 py-2"
                    >
                        Detalle
                    </Tabs.Tab>
                    <Tabs.Tab
                        value="historia"
                        ref={refHistoria}
                        leftSection={<IconHistory size={14} />}
                        className="relative z-10 px-4 py-2"
                    >
                        Historial
                    </Tabs.Tab>
                    <FloatingIndicator
                        target={tabIndicatorTarget}
                        parent={tabsListNode}
                        className="rounded-md transition-all"
                        style={{ zIndex: 0 }}
                    />
                </Tabs.List>

                <Tabs.Panel value="detalle" pt="md">
                    <Stack gap="lg">
                        <Grid gap="md">
                            <Grid.Col span={{ base: 12, md: 8 }}>
                                <DetailCard
                                    title="Datos del documento"
                                    description="Informacion principal de la venta."
                                    icon={<IconReceipt2 size={18} />}
                                    className="h-full"
                                >
                                    <Stack gap="lg">
                                        <Grid>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Cliente"
                                                    value={
                                                        documento.clienteNombre ??
                                                        "Cliente contado"
                                                    }
                                                />
                                            </Grid.Col>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Identificacion"
                                                    value={
                                                        documento.clienteIdentificacion
                                                    }
                                                />
                                            </Grid.Col>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Vendedor"
                                                    value={
                                                        documento.vendedorNombre
                                                    }
                                                />
                                            </Grid.Col>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Moneda"
                                                    value={
                                                        documento.monedaCodigo
                                                    }
                                                />
                                            </Grid.Col>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Tipo cambio"
                                                    value={documento.tipoCambio.toFixed(
                                                        6,
                                                    )}
                                                />
                                            </Grid.Col>
                                            <Grid.Col
                                                span={{
                                                    base: 12,
                                                    sm: 6,
                                                    md: 4,
                                                }}
                                            >
                                                <LabelValue
                                                    label="Vencimiento"
                                                    value={
                                                        documento.fechaVencimiento
                                                            ? formatFecha(
                                                                  documento.fechaVencimiento,
                                                              )
                                                            : null
                                                    }
                                                />
                                            </Grid.Col>
                                        </Grid>
                                        {documento.observaciones && (
                                            <>
                                                <Divider />
                                                <LabelValue
                                                    label="Observaciones"
                                                    value={
                                                        documento.observaciones
                                                    }
                                                />
                                            </>
                                        )}
                                        <Divider />
                                        <Stack gap="md">
                                            <Text fw={700} size="sm">
                                                Auditoria
                                            </Text>
                                            <Grid>
                                                <Grid.Col
                                                    span={{ base: 12, sm: 6 }}
                                                >
                                                    <LabelValue
                                                        label="Creado por"
                                                        value={
                                                            documento.creadoPor
                                                        }
                                                    />
                                                </Grid.Col>
                                                <Grid.Col
                                                    span={{ base: 12, sm: 6 }}
                                                >
                                                    <LabelValue
                                                        label="Fecha documento"
                                                        value={formatFecha(
                                                            documento.fechaDocumento,
                                                        )}
                                                    />
                                                </Grid.Col>
                                            </Grid>
                                        </Stack>
                                    </Stack>
                                </DetailCard>
                            </Grid.Col>

                            <Grid.Col span={{ base: 12, md: 4 }}>
                                <DetailCard
                                    title="Totales"
                                    description="Resumen monetario del documento."
                                    icon={<IconScale size={18} />}
                                    className="h-full sticky top-24"
                                >
                                    <Stack gap="sm">
                                        <Group justify="space-between">
                                            <Text c="dimmed" size="sm">
                                                Subtotal
                                            </Text>
                                            <Text fw={600}>
                                                {formatMonedaPorCodigo(
                                                    documento.totalVenta,
                                                    documento.monedaCodigo,
                                                )}
                                            </Text>
                                        </Group>
                                        <Group justify="space-between">
                                            <Text c="dimmed" size="sm">
                                                Descuentos
                                            </Text>
                                            <Text fw={600}>
                                                {formatMonedaPorCodigo(
                                                    documento.totalDescuentos,
                                                    documento.monedaCodigo,
                                                )}
                                            </Text>
                                        </Group>
                                        <Group justify="space-between">
                                            <Text c="dimmed" size="sm">
                                                Impuesto
                                            </Text>
                                            <Text fw={600}>
                                                {formatMonedaPorCodigo(
                                                    documento.totalImpuesto,
                                                    documento.monedaCodigo,
                                                )}
                                            </Text>
                                        </Group>
                                        {Math.abs(documento.montoRedondeo) >=
                                            0.005 && (
                                            <Group justify="space-between">
                                                <Text c="dimmed" size="sm">
                                                    Redondeo
                                                </Text>
                                                <Text fw={600}>
                                                    {formatMonedaPorCodigo(
                                                        documento.montoRedondeo,
                                                        documento.monedaCodigo,
                                                    )}
                                                </Text>
                                            </Group>
                                        )}
                                        <Divider />
                                        <Group justify="space-between">
                                            <Text fw={800}>Total</Text>
                                            <Text fw={900} size="lg">
                                                {formatMonedaPorCodigo(
                                                    documento.totalComprobante,
                                                    documento.monedaCodigo,
                                                )}
                                            </Text>
                                        </Group>
                                        {!esProforma &&
                                            documento.tipoDocumento !==
                                                TIPO_DOCUMENTO_VENTA.NotaCredito &&
                                            documento.tipoDocumento !==
                                                TIPO_DOCUMENTO_VENTA.NotaDebito && (
                                                <>
                                                    {mostrarPagoSaldo && (
                                                        <>
                                                            <Group justify="space-between">
                                                                <Text c="dimmed" size="sm">
                                                                    Pagado
                                                                </Text>
                                                                <Text fw={600}>
                                                                    {formatMonedaPorCodigo(
                                                                        documento.totalPagado,
                                                                        documento.monedaCodigo,
                                                                    )}
                                                                </Text>
                                                            </Group>
                                                            <Group justify="space-between">
                                                                <Text c="dimmed" size="sm">
                                                                    Saldo
                                                                </Text>
                                                                <Text fw={700}>
                                                                    {formatMonedaPorCodigo(
                                                                        documento.saldoPendiente,
                                                                        documento.monedaCodigo,
                                                                    )}
                                                                </Text>
                                                            </Group>
                                                        </>
                                                    )}
                                                    {(montoNotasCredito > 0 ||
                                                        montoNotasDebito > 0) && (
                                                        <>
                                                            <Divider />
                                                            {montoNotasDebito > 0 && (
                                                                <Group justify="space-between">
                                                                    <Text c="orange" size="sm">
                                                                        Notas de débito
                                                                    </Text>
                                                                    <Text c="orange" fw={600}>
                                                                        +{formatMonedaPorCodigo(
                                                                            montoNotasDebito,
                                                                            documento.monedaCodigo,
                                                                        )}
                                                                    </Text>
                                                                </Group>
                                                            )}
                                                            {montoNotasCredito > 0 && (
                                                                <Group justify="space-between">
                                                                    <Text c="red" size="sm">
                                                                        Notas de crédito
                                                                    </Text>
                                                                    <Text c="red" fw={600}>
                                                                        −{formatMonedaPorCodigo(
                                                                            montoNotasCredito,
                                                                            documento.monedaCodigo,
                                                                        )}
                                                                    </Text>
                                                                </Group>
                                                            )}
                                                            <Group justify="space-between">
                                                                <Text fw={700} size="sm">
                                                                    Total neto
                                                                </Text>
                                                                <Text fw={800}>
                                                                    {formatMonedaPorCodigo(
                                                                        Math.max(
                                                                            0,
                                                                            totalConCargos -
                                                                                montoNotasCredito,
                                                                        ),
                                                                        documento.monedaCodigo,
                                                                    )}
                                                                </Text>
                                                            </Group>
                                                            {anuladaPorNC && (
                                                                <Badge color="red" variant="light" fullWidth>
                                                                    Anulada por NC
                                                                </Badge>
                                                            )}
                                                        </>
                                                    )}
                                                </>
                                            )}
                                    </Stack>
                                </DetailCard>
                            </Grid.Col>
                        </Grid>

                        <DetailCard
                            title="Lineas"
                            description="Productos y servicios incluidos."
                            icon={<IconTable size={18} />}
                        >
                            <LineasTable
                                lineas={documento.lineas}
                                monedaCodigo={documento.monedaCodigo}
                            />
                        </DetailCard>

                        <DetailCard
                            title="Pagos"
                            description="Medios de pago aplicados a la venta."
                            icon={<IconReceipt2 size={18} />}
                        >
                            <PagosTable
                                pagos={documento.pagos}
                                monedaCodigo={documento.monedaCodigo}
                                documentoId={documento.id}
                                puedeEmitirRecibo={
                                    documento.tipoDocumento === TIPO_DOCUMENTO_VENTA.Apartado ||
                                    esFacturaCredito
                                }
                                puedeAnularAbono={permisos.anularAbono ?? false}
                                esFacturaCredito={esFacturaCredito}
                                onAnularPago={(pagoId) => setAnularAbonoModalPagoId(pagoId)}
                            />
                        </DetailCard>

                        <DetailCard
                            title="Documentos relacionados"
                            description="Origen y documentos generados desde esta venta."
                            icon={<IconLink size={18} />}
                        >
                            <Stack gap="md">
                                {documento.documentoOrigen && (
                                    <Stack gap="xs">
                                        <Text c="dimmed" size="sm">
                                            Documento origen
                                        </Text>
                                        <DocumentoRelacionadoLink
                                            documento={
                                                documento.documentoOrigen
                                            }
                                        />
                                    </Stack>
                                )}
                                {documento.documentosGenerados.length > 0 && (
                                    <Stack gap="xs">
                                        <Text c="dimmed" size="sm">
                                            {esProforma
                                                ? "Factura generada desde esta proforma"
                                                : "Documentos generados"}
                                        </Text>
                                        {documento.documentosGenerados.map(
                                            (relacionado) => (
                                                <DocumentoRelacionadoLink
                                                    key={relacionado.id}
                                                    documento={relacionado}
                                                />
                                            ),
                                        )}
                                    </Stack>
                                )}
                                {!documento.documentoOrigen &&
                                    documento.documentosGenerados.length ===
                                        0 && (
                                        <Text c="dimmed" size="sm">
                                            Este documento no tiene documentos
                                            relacionados.
                                        </Text>
                                    )}
                            </Stack>
                        </DetailCard>
                    </Stack>
                </Tabs.Panel>

                <Tabs.Panel value="historia" pt="md">
                    <HistoriaEventosVenta ventaId={documento.id} />
                </Tabs.Panel>
            </Tabs>
        </Stack>
    );
}
