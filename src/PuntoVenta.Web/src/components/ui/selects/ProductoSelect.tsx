"use client";

import { useEffect, useState } from "react";
import { Select, type ComboboxItem, type SelectProps } from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import {
    obtenerProductoPorIdService,
    obtenerProductosService,
} from "@lib/services/productos.service";

type ProductoSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage">;
const TIPO_ITEM_BIEN = 1;
const MIN_SEARCH_LENGTH = 2;

function formatProductoLabel(producto: { nombre: string; codigo: string }) {
    return producto.nombre;
}

export function ProductoSelect({
    value,
    onChange,
    disabled,
    ...props
}: ProductoSelectProps) {
    const [searchValue, setSearchValue] = useState("");
    const [debouncedSearch] = useDebouncedValue(searchValue, 300);
    const normalizedSearch = debouncedSearch.trim();
    const shouldSearch = normalizedSearch.length >= MIN_SEARCH_LENGTH;

    const productosQuery = useQuery({
        queryKey: QUERY_KEYS.productos.lista({
            numeroPagina: 1,
            tamanoPagina: 20,
            filtroDinamico: shouldSearch ? normalizedSearch : undefined,
            tipoItem: TIPO_ITEM_BIEN,
        }),
        queryFn: async () => {
            const res = await obtenerProductosService({
                numeroPagina: 1,
                tamanoPagina: 20,
                filtroDinamico: normalizedSearch,
                tipoItem: TIPO_ITEM_BIEN,
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

    if (selectedProductQuery.data) {
        options.push({
            value: selectedProductQuery.data.id,
            label: formatProductoLabel(selectedProductQuery.data),
        });
    }

    for (const producto of shouldSearch ? (productosQuery.data ?? []) : []) {
        if (options.some((option) => option.value === producto.id)) continue;

        options.push({
            value: producto.id,
            label: formatProductoLabel(producto),
        });
    }

    // searchValue es dual: el texto que teclea el usuario para buscar y el label
    // del producto seleccionado. El label sólo está disponible tras el fetch
    // async (selectedProductQuery), no se puede derivar en render. Ver override
    // en react-doctor.config.json para los selects async.
    useEffect(() => {
        if (!value) {
            setSearchValue("");
            return;
        }

        if (!selectedProductQuery.data) return;

        setSearchValue(formatProductoLabel(selectedProductQuery.data));
    }, [selectedProductQuery.data, value]);

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={disabled}
            searchable
            filter={({ options }) => options}
            searchValue={searchValue}
            onSearchChange={setSearchValue}
            value={value}
            onChange={(nextValue, option) => {
                onChange?.(nextValue, option);
                setSearchValue(option?.label ?? "");
            }}
            clearable={props.clearable}
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
