"use client";

import {
    DatePickerInput,
    type DatePickerInputProps,
} from "@mantine/dates";

export function AppDatePickerInput<
    Type extends "default" | "multiple" | "range" = "default",
>(props: DatePickerInputProps<Type>) {
    return (
        <DatePickerInput
            locale="es"
            valueFormat="DD/MM/YYYY"
            placeholder="Selecciona fecha"
            clearable
            size="sm"
            {...props}
        />
    );
}
