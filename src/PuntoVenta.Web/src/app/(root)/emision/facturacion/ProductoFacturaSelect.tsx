"use client";

import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import {
    Badge,
    Group,
    Select,
    Stack,
    Text,
    ThemeIcon,
    type ComboboxItem,
    type SelectProps,
} from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { IconBox, IconTag } from "@tabler/icons-react";
import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerProductoPorCodigoBarrasService,
    obtenerProductoPorIdService,
    obtenerProductosService,
} from "@lib/services/productos.service";
import type { ProductoDto } from "@lib/types/productos.types";
import { formatMonedaPorCodigo } from "@lib/utils/ventas.utils";
import {
    colorPorEstadoStock,
    colorPorTipoItem,
    etiquetaStock,
    obtenerEstadoStock,
} from "@lib/utils/productos.utils";

type ProductoFacturaSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage"> & {
    onProductoChange?: (producto: ProductoDto | null) => void;
    onProductoSeleccionado?: (producto: ProductoDto) => void;
    monedaCodigo?: string;
};

const MIN_SEARCH_LENGTH = 2;

function formatProductoLabel(producto: ProductoDto) {
    const tipo = producto.tipoItem === "Bien" ? "Bien" : "Servicio";
    return `${producto.nombre} • ${tipo}`;
}

function ProductoOption({
    producto,
    monedaCodigo,
}: {
    producto: ProductoDto;
    monedaCodigo: string;
}) {
    const esBien = producto.tipoItem === "Bien";
    const estadoStock = obtenerEstadoStock(producto);
    const labelStock = etiquetaStock(producto);

    return (
        <Group gap="sm" wrap="nowrap" className="w-full py-1">
            <ThemeIcon
                variant="light"
                radius="md"
                size={36}
                color={colorPorTipoItem(producto.tipoItem)}
            >
                {esBien ? <IconBox size={18} /> : <IconTag size={18} />}
            </ThemeIcon>
            <Stack gap={2} className="min-w-0 flex-1">
                <Text fw={600} size="sm" lineClamp={1}>
                    {producto.nombre}
                </Text>
                <Group gap={6} wrap="nowrap">
                    <Text size="xs" c="dimmed" ff="monospace">
                        {producto.codigo}
                    </Text>
                    {labelStock && (
                        <Badge
                            size="xs"
                            variant="light"
                            color={colorPorEstadoStock(estadoStock)}
                        >
                            {labelStock}
                        </Badge>
                    )}
                </Group>
            </Stack>
            <Text fw={700} size="sm" className="tabular-nums whitespace-nowrap">
                {formatMonedaPorCodigo(producto.precioUnitario, monedaCodigo)}
            </Text>
        </Group>
    );
}

export function ProductoFacturaSelect({
    value,
    onChange,
    onProductoChange,
    onProductoSeleccionado,
    disabled,
    monedaCodigo = "CRC",
    ...props
}: ProductoFacturaSelectProps) {
    const inputRef = useRef<HTMLInputElement>(null);
    const [searchValue, setSearchValue] = useState("");
    const [dropdownOpened, setDropdownOpened] = useState(false);
    const [debouncedSearch] = useDebouncedValue(searchValue, 300);
    const normalizedSearch = debouncedSearch.trim();
    const shouldSearch = normalizedSearch.length >= MIN_SEARCH_LENGTH;

    const productosQuery = useQuery({
        queryKey: QUERY_KEYS.productos.lista({
            numeroPagina: 1,
            tamanoPagina: 20,
            filtroDinamico: shouldSearch ? normalizedSearch : undefined,
        }),
        queryFn: async () => {
            const res = await obtenerProductosService({
                numeroPagina: 1,
                tamanoPagina: 20,
                filtroDinamico: normalizedSearch,
            });

            if (res.errors) throw res.errors;
            return res.data?.items ?? [];
        },
        enabled: shouldSearch,
        placeholderData: (previousData) => previousData,
    });

    const selectedProductQuery = useQuery({
        queryKey: value ? QUERY_KEYS.productos.detalle(value) : ["productos", "detalle", "empty"],
        queryFn: async () => {
            if (!value) return null;

            const res = await obtenerProductoPorIdService(value);
            if (res.errors) throw res.errors;
            return res.data ?? null;
        },
        enabled: Boolean(value),
        staleTime: 1000 * 60 * 5,
    });

    const options: ComboboxItem[] = [];
    const productosPorId = new Map<string, ProductoDto>();

    if (selectedProductQuery.data) {
        options.push({
            value: selectedProductQuery.data.id,
            label: formatProductoLabel(selectedProductQuery.data),
        });
        productosPorId.set(selectedProductQuery.data.id, selectedProductQuery.data);
    }

    for (const producto of shouldSearch ? (productosQuery.data ?? []) : []) {
        if (options.some((option) => option.value === producto.id)) continue;
        options.push({
            value: producto.id,
            label: formatProductoLabel(producto),
        });
        productosPorId.set(producto.id, producto);
    }

    useEffect(() => {
        if (!value) {
            setSearchValue("");
            onProductoChange?.(null);
            return;
        }

        if (!selectedProductQuery.data) return;

        setSearchValue(formatProductoLabel(selectedProductQuery.data));
        onProductoChange?.(selectedProductQuery.data);
    }, [onProductoChange, selectedProductQuery.data, value]);

    async function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
        if (event.key !== "Enter") return;
        if (!onProductoSeleccionado) return;
        const codigo = searchValue.trim();
        if (!codigo) return;

        // Solo confiamos en typeahead si el debounce ya disparó y la query
        // refleja exactamente el search actual. Sino el data esta stale (caso
        // tipico de pistola que termina con Enter antes del debounce).
        const debounceSincronizado = debouncedSearch.trim() === codigo && !productosQuery.isFetching;
        const matchExactoTypeahead = debounceSincronizado
            ? (productosQuery.data ?? []).find(
                  (p) => p.codigoBarras && p.codigoBarras.trim() === codigo,
              )
            : undefined;
        const hayOpcionesParcialesParaElegir =
            debounceSincronizado &&
            (productosQuery.data ?? []).length > 0 &&
            !matchExactoTypeahead;
        if (hayOpcionesParcialesParaElegir) return;

        event.preventDefault();
        event.stopPropagation();

        if (matchExactoTypeahead) {
            setSearchValue("");
            setDropdownOpened(false);
            onProductoSeleccionado(matchExactoTypeahead);
            setTimeout(() => inputRef.current?.focus(), 0);
            return;
        }

        const res = await obtenerProductoPorCodigoBarrasService(codigo);
        if (res.errors || !res.data) return;

        setSearchValue("");
        setDropdownOpened(false);
        onProductoSeleccionado(res.data);
        setTimeout(() => inputRef.current?.focus(), 0);
    }

    return (
        <Select
            ref={inputRef}
            comboboxProps={{ withinPortal: true, shadow: "md" }}
            data={options}
            disabled={disabled}
            searchable
            filter={({ options }) => options}
            renderOption={({ option }) => {
                const producto = productosPorId.get(option.value);
                if (!producto) return <Text size="sm">{option.label}</Text>;
                return (
                    <ProductoOption
                        producto={producto}
                        monedaCodigo={monedaCodigo}
                    />
                );
            }}
            dropdownOpened={dropdownOpened}
            onDropdownOpen={() => setDropdownOpened(true)}
            onDropdownClose={() => setDropdownOpened(false)}
            searchValue={searchValue}
            onSearchChange={setSearchValue}
            onKeyDown={handleKeyDown}
            value={value}
            onChange={(nextValue, option) => {
                onChange?.(nextValue, option);

                const producto = (productosQuery.data ?? []).find((item) => item.id === nextValue)
                    ?? (selectedProductQuery.data?.id === nextValue ? selectedProductQuery.data : null);
                onProductoChange?.(producto);

                if (nextValue && producto && onProductoSeleccionado) {
                    setSearchValue("");
                    onProductoSeleccionado(producto);
                    setTimeout(() => {
                        inputRef.current?.focus();
                        setDropdownOpened(true);
                    }, 0);
                } else {
                    setSearchValue(option?.label ?? "");
                }
            }}
            clearable
            nothingFoundMessage={
                normalizedSearch.length === 0
                    ? "Escriba para buscar"
                    : normalizedSearch.length < MIN_SEARCH_LENGTH
                      ? `Escriba al menos ${MIN_SEARCH_LENGTH} caracteres`
                      : "No se encontraron productos"
            }
            {...props}
        />
    );
}
