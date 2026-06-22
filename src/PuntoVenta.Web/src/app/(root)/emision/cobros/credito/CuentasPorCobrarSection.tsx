"use client";

import { usePatchReducer } from "@lib/hooks/usePatchReducer";
import {
    Badge,
    Box,
    Button,
    Group,
    Stack,
    Switch,
    Text,
} from "@mantine/core";
import { IconCash } from "@tabler/icons-react";
import { formatDate } from "@lib/utils/date.utils";
import { AbonarFacturaModal } from "./AbonarFacturaModal";
import {
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import { useFacturasCreditoQuery } from "@lib/hooks/useCreditoQuery";
import type { ColumnDefinition } from "@lib/types/base.types";
import type { FacturaCreditoResumenDto } from "@lib/types/ventas.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import { DataTable } from "@ui/table/DataTable";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { TableHeader } from "@ui/table/TableHeader";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";

interface Props {
    puedeAbonar: boolean;
}

export default function CuentasPorCobrarSection({ puedeAbonar }: Props) {
    const [{ page, pageSize, search, soloVencidas, facturaAbonar }, patchState] =
        usePatchReducer({
            page: 1,
            pageSize: TABLE_PAGE_SIZE_DEFAULT,
            search: "",
            soloVencidas: false,
            facturaAbonar: null as FacturaCreditoResumenDto | null,
        });

    const params = {
        pagina: page,
        tamano: pageSize,
        filtro: search || undefined,
        soloVencidas: soloVencidas || undefined,
    };

    const { data, isFetching, isError, refetch } = useFacturasCreditoQuery(params);

    const columns: ColumnDefinition<FacturaCreditoResumenDto>[] = [
        {
            key: "consecutivo",
            header: "Factura",
            cell: (f) => (
                <Stack gap={2}>
                    <Text fw={600}>{f.consecutivo ?? "—"}</Text>
                    <Text size="xs" c="dimmed">
                        {formatDate(f.fechaDocumento, "datetime")}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "clienteNombre",
            header: "Cliente",
            cell: (f) => (
                <Stack gap={2}>
                    <Text size="sm">{f.clienteNombre ?? "—"}</Text>
                    {f.clienteIdentificacion && (
                        <Text size="xs" c="dimmed">{f.clienteIdentificacion}</Text>
                    )}
                </Stack>
            ),
        },
        {
            key: "fechaVencimiento",
            header: "Vencimiento",
            cell: (f) => (
                <Stack gap={2}>
                    <Text size="sm">
                        {f.fechaVencimiento
                            ? formatDate(f.fechaVencimiento, "date")
                            : "—"}
                    </Text>
                    {f.esVencida ? (
                        <Badge color="red" variant="light" size="sm">
                            Vencida hace {f.diasAtraso}d
                        </Badge>
                    ) : (
                        <Badge color="green" variant="light" size="sm">
                            Vigente
                        </Badge>
                    )}
                </Stack>
            ),
        },
        {
            key: "totalComprobante",
            header: "Totales",
            align: "right",
            cell: (f) => (
                <Stack align="flex-end" gap={2}>
                    <Text size="sm">{formatMonedaPorCodigo(f.totalComprobante, "CRC")}</Text>
                    <Text size="xs" c="dimmed">
                        Pagado: {formatMonedaPorCodigo(f.totalPagado, "CRC")}
                    </Text>
                    <Text size="xs" c="orange" fw={600}>
                        Saldo: {formatMonedaPorCodigo(f.saldoPendiente, "CRC")}
                    </Text>
                </Stack>
            ),
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "right",
            width: 140,
            cell: (f) => (
                puedeAbonar ? (
                    <Button
                        size="xs"
                        variant="light"
                        leftSection={<IconCash size={14} />}
                        onClick={() => patchState({ facturaAbonar: f })}
                    >
                        Abonar
                    </Button>
                ) : (
                    <Text size="xs" c="dimmed">
                        Sin permiso
                    </Text>
                )
            ),
        },
    ];

    function resetPage() {
        patchState({ page: 1 });
    }

    function handlePageSizeChange(size: number) {
        patchState({ page: 1, pageSize: size });
    }

    return (
        <Box className="rounded-lg border border-theme-border-soft overflow-hidden h-page flex flex-col">
            <TableHeader>
                <Group justify="space-between" className="w-full">
                    <Group wrap="wrap">
                        <DynamicSearchInput
                            value={search}
                            onChange={(value) => {
                                resetPage();
                                patchState({ search: value });
                            }}
                            placeholder="Buscar factura o cliente..."
                            className="min-w-80 w-100"
                        />
                        <Switch
                            label="Solo vencidas"
                            checked={soloVencidas}
                            onChange={(e) => {
                                resetPage();
                                patchState({ soloVencidas: e.currentTarget.checked });
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
                    getRowId={(f) => f.id}
                    emptyText="No hay facturas a crédito"
                    error={isError ? "Error al cargar cuentas por cobrar" : undefined}
                />
            </TableBody>
            <TableFooter>
                <TablePagination
                    page={data?.pagina ?? page}
                    pageSize={data?.tamano ?? pageSize}
                    total={data?.totalRegistros ?? 0}
                    totalPages={data?.totalPaginas ?? 1}
                    onPageChange={(page) => patchState({ page })}
                    onPageSizeChange={handlePageSizeChange}
                    pageSizeOptions={TABLE_PAGE_SIZE_OPTIONS}
                />
            </TableFooter>

            {facturaAbonar && puedeAbonar && (
                <AbonarFacturaModal
                    factura={facturaAbonar}
                    onClose={() => patchState({ facturaAbonar: null })}
                    onSuccess={() => {
                        patchState({ facturaAbonar: null });
                        refetch();
                    }}
                />
            )}
        </Box>
    );
}
