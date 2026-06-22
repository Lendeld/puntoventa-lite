"use client";

import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import { AppDatePickerInput } from "@components/ui/dates/AppDatePickerInput";
import {
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import {
    useCatalogosVentasQuery,
    useDocumentosVentaQuery,
} from "@lib/hooks/useDocumentosVentaQuery";
import { useImprimirTicketAhora } from "@lib/printing/imprimir-ticket";
import { getVentaPdfUrl } from "@lib/printing/venta-printing";
import { ColumnDefinition } from "@lib/types/base.types";
import type { DocumentoVentaResumenDto } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { redondearMoneda } from "@lib/utils/number.utils";
import {
    Badge,
    Box,
    Group,
    Menu,
    Select,
    Stack,
    Text,
    Tooltip,
    UnstyledButton,
} from "@mantine/core";
import {
    IconDotsVertical,
    IconEdit,
    IconExternalLink,
    IconEye,
    IconFileTypePdf,
    IconPrinter,
} from "@tabler/icons-react";
import { DataTable } from "@ui/table/DataTable";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { TableHeader } from "@ui/table/TableHeader";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { parseAsInteger, parseAsString, useQueryState } from "nuqs";
import { useReturnUrlPersist } from "@lib/hooks/useReturnUrl";

function formatFechaFiltro(value: string | null) {
    if (!value) return undefined;
    return dayjs(value).startOf("day").toISOString();
}

function formatFechaFiltroHasta(value: string | null) {
    if (!value) return undefined;
    return dayjs(value).endOf("day").toISOString();
}

const TIPO_DOCUMENTO_PROFORMA = "Proforma";
const TIPO_DOCUMENTO_NOTA_CREDITO = "NotaCredito";
const TIPO_DOCUMENTO_NOTA_DEBITO = "NotaDebito";
const ESTADO_DOCUMENTO_BORRADOR = "Borrador";

// Celda "Totales": muestra una cifra principal (el neto que importa) y, si hay
// ajustes o saldo, una sola línea de chips compactos. El desglose completo
// (Total, Pagado, Saldo, ND, NC, Neto) vive en el tooltip para no saturar la
// tabla en muchas filas.
function CeldaTotales({ documento }: { documento: DocumentoVentaResumenDto }) {
    const esComprobanteSimple =
        documento.tipoDocumento === TIPO_DOCUMENTO_PROFORMA ||
        documento.tipoDocumento === TIPO_DOCUMENTO_NOTA_CREDITO ||
        documento.tipoDocumento === TIPO_DOCUMENTO_NOTA_DEBITO;

    if (esComprobanteSimple) {
        return (
            <Text size="sm" className="tabular-nums font-semibold whitespace-nowrap" ta="right">
                {formatMonedaPorCodigo(documento.totalComprobante, documento.monedaCodigo)}
            </Text>
        );
    }

    const tieneAjustes =
        documento.montoNotasDebito > 0 || documento.montoNotasCredito > 0;
    const neto = Math.max(
        0,
        documento.totalComprobante +
            documento.montoNotasDebito -
            documento.montoNotasCredito,
    );
    const anulada =
        documento.montoNotasCredito >=
        documento.totalComprobante + documento.montoNotasDebito - 0.005;
    const cifraPrincipal = tieneAjustes ? neto : documento.totalComprobante;
    // Comparar a escala de pago: el saldo interno puede traer residuo
    // sub-céntimo (total 5 dec vs pago 2 dec) que no es saldo real en contado.
    const tieneSaldo = redondearMoneda(documento.saldoPendiente) > 0;
    // Pagado/Saldo solo aporta info cuando el pago es diferido (crédito o
    // apartado con saldo). En contado pagado al instante son redundantes.
    const mostrarPagoSaldo = documento.esCredito || tieneSaldo;

    const fmt = (v: number) => formatMonedaPorCodigo(v, documento.monedaCodigo);

    const detalle = (
        <Stack gap={2} className="tabular-nums">
            <Group justify="space-between" gap="lg">
                <Text size="xs" c="dimmed">Total</Text>
                <Text size="xs">{fmt(documento.totalComprobante)}</Text>
            </Group>
            {mostrarPagoSaldo && (
                <>
                    <Group justify="space-between" gap="lg">
                        <Text size="xs" c="dimmed">Pagado</Text>
                        <Text size="xs">{fmt(documento.totalPagado)}</Text>
                    </Group>
                    <Group justify="space-between" gap="lg">
                        <Text size="xs" c="dimmed">Saldo</Text>
                        <Text size="xs">{fmt(documento.saldoPendiente)}</Text>
                    </Group>
                </>
            )}
            {documento.montoNotasDebito > 0 && (
                <Group justify="space-between" gap="lg">
                    <Text size="xs" c="dimmed">Notas de débito</Text>
                    <Text size="xs" c="orange">+{fmt(documento.montoNotasDebito)}</Text>
                </Group>
            )}
            {documento.montoNotasCredito > 0 && (
                <Group justify="space-between" gap="lg">
                    <Text size="xs" c="dimmed">Notas de crédito</Text>
                    <Text size="xs" c="red">−{fmt(documento.montoNotasCredito)}</Text>
                </Group>
            )}
            {tieneAjustes && (
                <Group justify="space-between" gap="lg">
                    <Text size="xs" fw={700}>Neto</Text>
                    <Text size="xs" fw={700}>{fmt(neto)}</Text>
                </Group>
            )}
        </Stack>
    );

    return (
        <Tooltip
            label={detalle}
            withArrow
            multiline
            position="left"
            color="dark"
        >
            <Stack align="flex-end" gap={3} className="cursor-default whitespace-nowrap">
                <Text size="sm" className="tabular-nums font-semibold">
                    {fmt(cifraPrincipal)}
                </Text>
                {anulada ? (
                    <Badge
                        size="xs"
                        color="red"
                        variant="light"
                        classNames={{ label: "overflow-visible!" }}
                    >
                        Anulada
                    </Badge>
                ) : tieneAjustes ? (
                    <Group gap={4} justify="flex-end">
                        {documento.montoNotasDebito > 0 && (
                            <Badge size="xs" color="orange" variant="light" className="tabular-nums">
                                +ND
                            </Badge>
                        )}
                        {documento.montoNotasCredito > 0 && (
                            <Badge size="xs" color="red" variant="light" className="tabular-nums">
                                −NC
                            </Badge>
                        )}
                    </Group>
                ) : tieneSaldo ? (
                    <Text c="orange" size="xs" className="tabular-nums">
                        Saldo {fmt(documento.saldoPendiente)}
                    </Text>
                ) : null}
            </Stack>
        </Tooltip>
    );
}

export default function VentasPageSection() {
    const imprimirTicket = useImprimirTicketAhora();
    const hoy = dayjs().format("YYYY-MM-DD");
    // URL query state: filtros persisten en URL -> back/forward y bookmark
    // funcionan, copy/paste link comparte estado. Defaults se omiten de URL
    // (withDefault no escribe si valor == default).
    const [page, setPage] = useQueryState("page", parseAsInteger.withDefault(1));
    const [pageSize, setPageSize] = useQueryState("size", parseAsInteger.withDefault(TABLE_PAGE_SIZE_DEFAULT));
    const [search, setSearch] = useQueryState("q", parseAsString.withDefault(""));
    const [tipoDocumento, setTipoDocumento] = useQueryState("tipo", parseAsString.withDefault(""));
    const [estado, setEstado] = useQueryState("estado", parseAsString.withDefault(""));
    const [desde, setDesde] = useQueryState("desde", parseAsString.withDefault(hoy));
    const [hasta, setHasta] = useQueryState("hasta", parseAsString.withDefault(hoy));

    // Datepicker maneja estado intermedio local (puede ser [X, null] mid-pick).
    // Commit a URL solo cuando ambos no-null.
    const [rangoFechas, setRangoFechas] = useState<
        [string | null, string | null]
    >([desde, hasta]);
    // Sincroniza picker con URL en back/forward (la URL cambia, useState init
    // no re-corre). Sin esto picker queda stale aunque la query sí refetcha.
    // react-doctor-disable-next-line react-doctor/no-derived-state-effect
    useEffect(() => {
        setRangoFechas([desde, hasta]);
    }, [desde, hasta]);

    // Persiste URL (con filtros) para que detalle pueda volver con estado.
    useReturnUrlPersist("ventas");
    const rangoAplicado: [string, string] = [desde, hasta];

    const onRangoChange = (v: [string | null, string | null]) => {
        setRangoFechas(v);
        if (v[0] !== null && v[1] !== null) {
            void setDesde(v[0]);
            void setHasta(v[1]);
        }
    };

    const params = {
        numeroPagina: page,
        tamanoPagina: pageSize,
        filtroDinamico: search || undefined,
        tipoDocumento: tipoDocumento ? parseInt(tipoDocumento, 10) : undefined,
        estado: estado ? parseInt(estado, 10) : undefined,
        fechaDesde: formatFechaFiltro(rangoAplicado[0]),
        fechaHasta: formatFechaFiltroHasta(rangoAplicado[1]),
    };

    const { data, isFetching, isError, refetch } = useDocumentosVentaQuery(params);
    const { data: catalogosVentas } = useCatalogosVentasQuery();

    const tipoDocumentoOptions = [
        { value: "", label: "Todos los tipos" },
        ...(catalogosVentas?.tiposDocumento.map((item) => ({
            value: item.valor.toString(),
            label: item.detalle,
        })) ?? []),
    ];

    const estadoDocumentoOptions = [
        { value: "", label: "Todos los estados" },
        ...(catalogosVentas?.estadosDocumento.map((item) => ({
            value: item.valor.toString(),
            label: item.detalle,
        })) ?? []),
    ];

    const columns: ColumnDefinition<DocumentoVentaResumenDto>[] = [
        {
            key: "consecutivo",
            header: "Documento",
            cell: (documento) => (
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
                    <Text fw={600}>
                        {documento.consecutivo ?? "Sin consecutivo"}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "fechaDocumento",
            header: "Fecha",
            cell: (documento) => (
                <AuditDateHoverCard
                    date={documento.fechaDocumento}
                    title="Creado por"
                    user={documento.creadoPor}
                />
            ),
        },
        {
            key: "condicionVentaDetalleSnapshot",
            header: "Condición",
            cell: (documento) => (
                <Text size="sm">{documento.condicionVentaDetalleSnapshot}</Text>
            ),
        },
        {
            key: "clienteId",
            header: "Cliente",
            cell: (documento) => (
                <Stack gap={2}>
                    <Text size="sm">
                        {documento.clienteNombre ?? "Cliente contado"}
                    </Text>
                    {documento.clienteIdentificacion && (
                        <Text c="dimmed" size="xs">
                            {documento.clienteIdentificacion}
                        </Text>
                    )}
                </Stack>
            ),
        },
        {
            key: "totalComprobante",
            header: "Totales",
            align: "right",
            // Ancho fit-content: colapsa al ancho del contenido y crece si el
            // monto es más largo. El nowrap evita que la cifra parta en 2 líneas.
            width: "1%",
            cell: (documento) => <CeldaTotales documento={documento} />,
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "right",
            width: 80,
            cell: (documento) => (
                <Group justify="flex-end" gap={6} wrap="nowrap">
                    <Menu shadow="md" width={210} position="bottom-end">
                        <Menu.Target>
                            <UnstyledButton
                                className="inline-flex items-center justify-center rounded-md p-1.5 text-theme-text-muted transition-colors hover:bg-theme-surface-2 hover:text-theme-text"
                                aria-label="Abrir acciones de venta"
                            >
                                <IconDotsVertical size={18} />
                            </UnstyledButton>
                        </Menu.Target>
                        <Menu.Dropdown>
                            <Menu.Item
                                component="a"
                                href={`/emision/ventas/${documento.id}`}
                                leftSection={<IconEye size={16} />}
                            >
                                Ver detalle
                            </Menu.Item>
                            <Menu.Divider />
                            {documento.tipoDocumento === TIPO_DOCUMENTO_PROFORMA &&
                                documento.estado === ESTADO_DOCUMENTO_BORRADOR && (
                                    <>
                                        <Menu.Item
                                            component="a"
                                            href={`/emision/facturacion?proformaId=${documento.id}`}
                                            leftSection={<IconEdit size={16} />}
                                        >
                                            Editar proforma
                                        </Menu.Item>
                                        <Menu.Divider />
                                    </>
                                )}
                            <Menu.Item
                                component="a"
                                href={getVentaPdfUrl(documento.id)}
                                target="_blank"
                                rel="noopener noreferrer"
                                leftSection={<IconFileTypePdf size={16} />}
                                rightSection={<IconExternalLink size={14} />}
                            >
                                Ver PDF
                            </Menu.Item>
                            <Menu.Divider />
                            <Menu.Item
                                leftSection={<IconPrinter size={16} />}
                                onClick={() => imprimirTicket(documento.id)}
                            >
                                Imprimir ticket
                            </Menu.Item>
                        </Menu.Dropdown>
                    </Menu>
                </Group>
            ),
        },
    ];

    function resetPage() {
        setPage(1);
    }

    function handlePageSizeChange(size: number) {
        setPage(1);
        setPageSize(size);
    }

    return (
        <Box className="rounded-lg border border-theme-border-soft bg-theme-surface overflow-hidden h-page flex flex-col">
            <TableHeader>
                <Group justify="space-between" className="w-full">
                    <Group wrap="wrap">
                        <DynamicSearchInput
                            value={search}
                            onChange={(value) => {
                                resetPage();
                                setSearch(value);
                            }}
                            placeholder="Buscar documento..."
                            className="min-w-80 w-100"
                        />
                        <Select
                            data={tipoDocumentoOptions}
                            value={tipoDocumento}
                            onChange={(value) => {
                                resetPage();
                                setTipoDocumento(value ?? "");
                            }}
                            size="sm"
                            w={180}
                            allowDeselect={false}
                        />
                        <Select
                            data={estadoDocumentoOptions}
                            value={estado}
                            onChange={(value) => {
                                resetPage();
                                setEstado(value ?? "");
                            }}
                            size="sm"
                            w={180}
                            allowDeselect={false}
                        />
                        <AppDatePickerInput
                            allowSingleDateInRange
                            clearable={false}
                            highlightToday
                            type="range"
                            w={260}
                            value={rangoFechas}
                            onChange={(value) => {
                                resetPage();
                                onRangoChange(value);
                            }}
                        />
                        <TableRefreshButton
                            onClick={() => refetch()}
                            loading={isFetching}
                        />
                    </Group>
                </Group>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(documento) => documento.id}
                    emptyText="No hay ventas registradas"
                    error={isError ? "Error al cargar la bandeja de ventas" : undefined}
                />
            </TableBody>
            <TableFooter>
                <TablePagination
                    page={data?.pagina ?? page}
                    pageSize={data?.tamano ?? pageSize}
                    total={data?.totalRegistros ?? 0}
                    totalPages={data?.totalPaginas ?? 1}
                    onPageChange={setPage}
                    onPageSizeChange={handlePageSizeChange}
                    pageSizeOptions={TABLE_PAGE_SIZE_OPTIONS}
                />
            </TableFooter>
        </Box>
    );
}
