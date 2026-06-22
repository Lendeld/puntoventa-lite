"use client";

import { PermisoClient } from "@components/auth/PermisoClient";
import { AuditDateHoverCard } from "@components/ui/AuditDateHoverCard";
import { PERMISOS } from "@lib/constants/permisos.constants";
import {
    TABLE_PAGE_SIZE_DEFAULT,
    TABLE_PAGE_SIZE_OPTIONS,
} from "@lib/constants/table.constants";
import { useProductosQuery } from "@lib/hooks/useProductosQuery";
import { useCategoriasActivasQuery } from "@lib/hooks/useCategoriasActivasQuery";
import { useTarifasIvaActivasQuery } from "@lib/hooks/useTarifasIvaActivasQuery";
import { ColumnDefinition } from "@lib/types/base.types";
import type { ProductoDto } from "@lib/types/productos.types";
import type { CategoriaDto } from "@lib/types/categorias.types";
import type { TarifaIvaImpuestoDto } from "@lib/types/configuracion.types";
import { redondear } from "@lib/utils/number.utils";
import {
    Badge,
    Box,
    Button,
    Group,
    Indicator,
    Pill,
    Popover,
    Select,
    Stack,
    Text,
} from "@mantine/core";
import { IconFilter, IconChevronDown } from "@tabler/icons-react";
import MenuProductoAcciones from "@pages/inventario/productos/MenuProductoAcciones";
import NewProductoSection from "@pages/inventario/productos/NewProductoSection";
import { DataTable } from "@ui/table/DataTable";
import { DynamicSearchInput } from "@ui/table/DynamicSearchInput";
import { TableBody } from "@ui/table/TableBody";
import { TableFooter } from "@ui/table/TableFooter";
import { TableHeader } from "@ui/table/TableHeader";
import { TablePagination } from "@ui/table/TablePagination";
import { TableRefreshButton } from "@ui/table/TableRefreshButton";
import { usePatchReducer } from "@lib/hooks/usePatchReducer";

const TIPO_ITEM_FILTER_OPTIONS = [
    { value: "", label: "Todos los tipos" },
    { value: "1", label: "Bien" },
    { value: "2", label: "Servicio" },
];

// Backend serializa enum como string ("Bien"/"Servicio") via JsonStringEnumConverter.
const TIPO_ITEM_LABELS: Record<string, string> = { Bien: "Bien", Servicio: "Servicio" };
// Bien = orange (warm, armoniza con café del theme). Servicio = teal (cool complementario).
const TIPO_ITEM_COLORS: Record<string, string> = { Bien: "orange", Servicio: "teal" };

function formatMoneda(n: number) {
    return `₡ ${n.toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

interface Props {
    puedeCrear: boolean;
    puedeEditar: boolean;
    puedeAjustarStock: boolean;
}

// La sección coordina filtros URL-like, columnas y paginación de una sola tabla.
// react-doctor-disable-next-line react-doctor/no-giant-component
export default function ProductosPageSection({
    puedeCrear,
    puedeEditar,
    puedeAjustarStock,
}: Props) {
    const [
        { page, pageSize, search, tipoItemFiltro, categoriaFiltro },
        patchState,
    ] = usePatchReducer({
        page: 1,
        pageSize: TABLE_PAGE_SIZE_DEFAULT,
        search: "",
        tipoItemFiltro: "",
        categoriaFiltro: null as string | null,
    });

    const params = {
        numeroPagina: page,
        tamanoPagina: pageSize,
        filtroDinamico: search || undefined,
        tipoItem: tipoItemFiltro ? parseInt(tipoItemFiltro) : undefined,
        categoriaId: categoriaFiltro ?? undefined,
    };

    const { data, isFetching, isError, refetch } = useProductosQuery(params);

    const { data: tarifasIva } = useTarifasIvaActivasQuery();
    const { data: categorias } = useCategoriasActivasQuery();

    const tarifasMap = new Map<string, number>(
        (tarifasIva ?? []).map((t: TarifaIvaImpuestoDto) => [t.codigo, t.porcentaje])
    );
    const categoriasMap = new Map<string, string>(
        (categorias ?? []).map((c: CategoriaDto) => [c.id, c.nombre])
    );

    function calcPrecioVenta(producto: ProductoDto): number {
        const pct = tarifasMap.get(producto.tarifaIvaImpuestoCodigo ?? "") ?? 0;
        return redondear(producto.precioUnitario * (1 + pct / 100));
    }

    const columns: ColumnDefinition<ProductoDto>[] = [
        {
            key: "nombre",
            header: "Producto",
            cell: (p) => (
                <Stack gap={2} py={4}>
                    <Group gap="xs">
                        <Text size="xs" c="dimmed" ff="monospace">{p.codigo}</Text>
                        <Badge
                            size="xs"
                            variant="light"
                            color={TIPO_ITEM_COLORS[p.tipoItem] ?? "gray"}
                        >
                            {TIPO_ITEM_LABELS[p.tipoItem] ?? p.tipoItem}
                        </Badge>
                    </Group>
                    <Group gap="xs" wrap="nowrap">
                        <Text fw={600} size="sm" lineClamp={1}>{p.nombre}</Text>
                    </Group>
                    {p.descripcion && (
                        <Text size="xs" c="dimmed" lineClamp={2}>{p.descripcion}</Text>
                    )}
                </Stack>
            ),
        },
        {
            key: "categoriaId",
            header: "Categoría",
            cell: (p) => (
                <Text size="sm">{categoriasMap.get(p.categoriaId ?? "") ?? "—"}</Text>
            ),
        },
        {
            key: "precioUnitario",
            header: "Precios",
            align: "right",
            cell: (p) => (
                <Stack gap={1} align="flex-end">
                    <Group gap={4} justify="flex-end">
                        <Text size="xs" c="dimmed">Costo:</Text>
                        <Text size="xs" c="dimmed" className="tabular-nums">
                            {p.precioCosto != null ? formatMoneda(p.precioCosto) : "—"}
                        </Text>
                    </Group>
                    <Group gap={4} justify="flex-end">
                        <Text size="sm" c="dimmed">Neto:</Text>
                        <Text size="sm" className="tabular-nums">
                            {formatMoneda(p.precioUnitario)}
                        </Text>
                    </Group>
                    <Group gap={4} justify="flex-end">
                        <Text size="sm" c="dimmed">Venta:</Text>
                        <Text size="sm" fw={600} className="tabular-nums">
                            {formatMoneda(calcPrecioVenta(p))}
                        </Text>
                    </Group>
                </Stack>
            ),
        },
        {
            key: "fechaCreacion",
            header: "Creación",
            cell: (p) => <AuditDateHoverCard date={p.fechaCreacion} title="Creado" />,
        },
        {
            key: "acciones",
            header: "Acciones",
            align: "center",
            cell: (p) => (
                <MenuProductoAcciones
                    id={p.id}
                    nombre={p.nombre}
                    puedeEditar={puedeEditar}
                    puedeAjustarStock={puedeAjustarStock && p.tipoItem === "Bien" && !p.noAplicaExistencias}
                />
            ),
        },
    ];

    const categoriasFilterOptions = [
        { value: "", label: "Todas las categorías" },
        ...(categorias ?? []).map((c) => ({ value: c.id, label: c.nombre })),
    ];

    function handleSearchChange(value: string) {
        patchState({ page: 1, search: value });
    }

    function handlePageSizeChange(size: number) {
        patchState({ page: 1, pageSize: size });
    }

    const tipoLabel = TIPO_ITEM_FILTER_OPTIONS.find((o) => o.value === tipoItemFiltro)?.label;
    const categoriaLabel = categorias?.find((c) => c.id === categoriaFiltro)?.nombre;

    const filtrosActivos = [
        tipoItemFiltro && {
            key: "tipo",
            label: `Tipo: ${tipoLabel}`,
            clear: () => patchState({ page: 1, tipoItemFiltro: "" }),
        },
        categoriaFiltro && {
            key: "categoria",
            label: categoriaLabel ?? "Categoría",
            clear: () => patchState({ page: 1, categoriaFiltro: null }),
        },
    ].filter(Boolean) as { key: string; label: string; clear: () => void }[];

    const numFiltros = filtrosActivos.length;

    function limpiarFiltros() {
        patchState({
            page: 1,
            tipoItemFiltro: "",
            categoriaFiltro: null,
        });
    }

    return (
        <Box className="rounded-lg border border-theme-border-soft bg-theme-surface overflow-hidden h-page flex flex-col">
            <TableHeader>
                <Stack gap="xs" className="w-full">
                    <Group justify="space-between" className="w-full" wrap="nowrap">
                        <Group wrap="wrap" gap="sm">
                            <DynamicSearchInput
                                value={search}
                                onChange={handleSearchChange}
                                placeholder="Buscar producto..."
                                className="min-w-60 w-80"
                            />
                            <Popover position="bottom-start" shadow="md" width={260}>
                                <Popover.Target>
                                    <Indicator
                                        label={numFiltros}
                                        size={16}
                                        color="accentPV"
                                        disabled={numFiltros === 0}
                                        offset={4}
                                    >
                                        <Button
                                            variant="light"
                                            color="accentPV"
                                            leftSection={<IconFilter size={16} />}
                                            rightSection={<IconChevronDown size={14} />}
                                        >
                                            Filtros
                                        </Button>
                                    </Indicator>
                                </Popover.Target>
                                <Popover.Dropdown>
                                    <Stack gap="sm">
                                        <Select
                                            label="Tipo de ítem"
                                            data={TIPO_ITEM_FILTER_OPTIONS}
                                            value={tipoItemFiltro}
                                            onChange={(val) => patchState({ page: 1, tipoItemFiltro: val ?? "" })}
                                            size="sm"
                                            allowDeselect={false}
                                        />
                                        <Select
                                            label="Categoría"
                                            data={categoriasFilterOptions}
                                            value={categoriaFiltro ?? ""}
                                            onChange={(val) => patchState({ page: 1, categoriaFiltro: val || null })}
                                            size="sm"
                                            searchable
                                            allowDeselect={false}
                                        />
                                        {numFiltros > 0 && (
                                            <Button
                                                variant="light"
                                                color="gray"
                                                size="xs"
                                                onClick={limpiarFiltros}
                                            >
                                                Limpiar filtros
                                            </Button>
                                        )}
                                    </Stack>
                                </Popover.Dropdown>
                            </Popover>
                            <TableRefreshButton onClick={() => refetch()} loading={isFetching} />
                        </Group>
                        {puedeCrear && (
                            <PermisoClient permiso={PERMISOS.PRODUCTOS_CREAR}>
                                <NewProductoSection />
                            </PermisoClient>
                        )}
                    </Group>
                    {numFiltros > 0 && (
                        <Group gap="xs" wrap="wrap">
                            {filtrosActivos.map((f) => (
                                <Pill
                                    key={f.key}
                                    withRemoveButton
                                    onRemove={f.clear}
                                    size="md"
                                >
                                    {f.label}
                                </Pill>
                            ))}
                        </Group>
                    )}
                </Stack>
            </TableHeader>
            <TableBody>
                <DataTable
                    columns={columns}
                    data={data?.items ?? []}
                    loading={isFetching}
                    getRowId={(p) => p.id}
                    emptyText="No hay productos"
                    error={isError ? "Error al cargar los productos" : undefined}
                />
            </TableBody>
            <TableFooter>
                <TablePagination
                    page={data?.pagina ?? page}
                    pageSize={data?.tamano ?? pageSize}
                    total={data?.totalRegistros ?? 0}
                    totalPages={data?.totalPaginas ?? 1}
                    onPageChange={(p) => patchState({ page: p })}
                    onPageSizeChange={handlePageSizeChange}
                    pageSizeOptions={TABLE_PAGE_SIZE_OPTIONS}
                />
            </TableFooter>
        </Box>
    );
}
