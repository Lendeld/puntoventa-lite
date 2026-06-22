"use client";

import { useEffect, useState } from "react";
import { CloseButton, TextInput } from "@mantine/core";
import { useDebouncedCallback } from "@mantine/hooks";
import { IconSearch } from "@tabler/icons-react";

interface Props {
    value: string;
    onChange: (value: string) => void;
    placeholder?: string;
    className?: string;
}

export function DynamicSearchInput({
    value,
    onChange,
    placeholder = "Buscar...",
    className,
}: Props) {
    // Estado local del texto: muestra el tecleo al instante mientras el `onChange`
    // hacia el padre se debouncea. Es el patrón inherente de un buscador con
    // debounce-on-commit, no un derived-state evitable.
    // react-doctor-disable-next-line react-doctor/no-derived-useState
    const [inputValue, setInputValue] = useState(value);
    const debouncedOnChange = useDebouncedCallback(onChange, 350);

    // Sincroniza cuando el padre limpia el filtro desde afuera (ej: "limpiar
    // filtros"); el valor sigue siendo controlado por el padre en ese caso.
    // react-doctor-disable-next-line react-doctor/no-event-handler
    useEffect(() => {
        // react-doctor-disable-next-line react-doctor/no-derived-state
        if (value === "") setInputValue("");
    }, [value]);

    function handleChange(next: string) {
        setInputValue(next);
        debouncedOnChange(next);
    }

    return (
        <TextInput
            value={inputValue}
            onChange={(e) => handleChange(e.currentTarget.value)}
            placeholder={placeholder}
            className={className}
            leftSection={<IconSearch size={16} />}
            rightSection={
                inputValue ? (
                    <CloseButton
                        size="sm"
                        onClick={() => {
                            setInputValue("");
                            onChange("");
                        }}
                        aria-label="Limpiar búsqueda"
                    />
                ) : null
            }
        />
    );
}
