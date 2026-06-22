"use client";

import { useEffect, useState } from "react";
import { Select, type ComboboxItem, type SelectProps } from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { obtenerClientePorIdService, obtenerClientesService } from "@lib/services/clientes.service";
import type { ClienteListaDto } from "@lib/types/clientes.types";

type ClienteFacturaSelectProps = Omit<SelectProps, "data" | "nothingFoundMessage"> & {
    onClienteChange?: (cliente: ClienteListaDto | null) => void;
};

const MIN_SEARCH_LENGTH = 2;

function formatClienteLabel(cliente: Pick<ClienteListaDto, "nombre" | "identificacion">) {
    return cliente.nombre;
}

export function ClienteFacturaSelect({
    value,
    onChange,
    onClienteChange,
    disabled,
    ...props
}: ClienteFacturaSelectProps) {
    const [searchValue, setSearchValue] = useState("");
    const [debouncedSearch] = useDebouncedValue(searchValue, 300);
    const normalizedSearch = debouncedSearch.trim();
    const shouldSearch = normalizedSearch.length >= MIN_SEARCH_LENGTH;

    const clientesQuery = useQuery({
        queryKey: QUERY_KEYS.clientes.lista({
            numeroPagina: 1,
            tamanoPagina: 20,
            filtroDinamico: shouldSearch ? normalizedSearch : undefined,
            activo: true,
        }),
        queryFn: async () => {
            const res = await obtenerClientesService({
                numeroPagina: 1,
                tamanoPagina: 20,
                filtroDinamico: normalizedSearch,
                activo: true,
            });

            if (res.errors) throw res.errors;
            return res.data?.items ?? [];
        },
        enabled: shouldSearch,
        placeholderData: (previousData) => previousData,
    });

    const selectedClienteQuery = useQuery({
        queryKey: value ? QUERY_KEYS.clientes.detalle(value) : ["clientes", "detalle", "empty"],
        queryFn: async () => {
            if (!value) return null;

            const res = await obtenerClientePorIdService(value);
            if (res.errors) throw res.errors;
            return res.data ?? null;
        },
        enabled: Boolean(value),
        staleTime: 1000 * 60 * 5,
    });

    const options: ComboboxItem[] = [
        { value: "", label: "Sin cliente" },
    ];

    if (selectedClienteQuery.data) {
        options.push({
            value: selectedClienteQuery.data.id,
            label: formatClienteLabel(selectedClienteQuery.data),
        });
    }

    for (const cliente of shouldSearch ? (clientesQuery.data ?? []) : []) {
        if (options.some((option) => option.value === cliente.id)) continue;
        options.push({
            value: cliente.id,
            label: formatClienteLabel(cliente),
        });
    }

    useEffect(() => {
        if (!value) {
            setSearchValue("");
            onClienteChange?.(null);
            return;
        }

        if (!selectedClienteQuery.data) return;

        setSearchValue(formatClienteLabel(selectedClienteQuery.data));
        onClienteChange?.(selectedClienteQuery.data);
    }, [onClienteChange, selectedClienteQuery.data, value]);

    return (
        <Select
            comboboxProps={{ withinPortal: true }}
            data={options}
            disabled={disabled}
            searchable
            searchValue={searchValue}
            onSearchChange={setSearchValue}
            value={value}
            onChange={(nextValue, option) => {
                onChange?.(nextValue, option);
                setSearchValue(option?.label === "Sin cliente" ? "" : (option?.label ?? ""));

                const cliente = (clientesQuery.data ?? []).find((item) => item.id === nextValue)
                    ?? (selectedClienteQuery.data?.id === nextValue ? selectedClienteQuery.data : null);
                onClienteChange?.(cliente);
            }}
            clearable={false}
            nothingFoundMessage={
                normalizedSearch.length === 0
                    ? "Escriba para buscar"
                    : normalizedSearch.length < MIN_SEARCH_LENGTH
                      ? `Escriba al menos ${MIN_SEARCH_LENGTH} caracteres`
                      : "No se encontraron clientes"
            }
            {...props}
        />
    );
}
