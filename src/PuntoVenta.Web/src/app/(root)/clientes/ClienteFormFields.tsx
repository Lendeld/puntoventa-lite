"use client";

import { CLIENTE_FIELDS } from "@lib/constants/clientes.constants";
import type {
    ActualizarClienteFormValues,
    CrearClienteFormValues,
} from "@lib/types/clientes.types";
import {
    Grid,
    TextInput,
    Textarea,
} from "@mantine/core";
import type { UseFormReturnType } from "@mantine/form";

const NOMBRE_MAX = 100;
const IDENTIFICACION_MAX = 20;
const CORREO_MAX = 160;
const TELEFONO_MAX = 20;
const OBSERVACIONES_MAX = 500;

type ClienteFormShape = CrearClienteFormValues | ActualizarClienteFormValues;

interface Props {
    form: UseFormReturnType<ClienteFormShape>;
}

export function ClienteFormFields({ form }: Props) {
    return (
        <Grid gap="md">
            <Grid.Col span={12}>
                <TextInput
                    label="Nombre"
                    placeholder="Nombre o razón social"
                    required
                    maxLength={NOMBRE_MAX}
                    key={form.key(CLIENTE_FIELDS.NOMBRE)}
                    {...form.getInputProps(CLIENTE_FIELDS.NOMBRE)}
                />
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 6 }}>
                <TextInput
                    label="Identificación"
                    placeholder="Número de identificación"
                    maxLength={IDENTIFICACION_MAX}
                    key={form.key(CLIENTE_FIELDS.IDENTIFICACION)}
                    {...form.getInputProps(CLIENTE_FIELDS.IDENTIFICACION)}
                />
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 6 }}>
                <TextInput
                    label="Correo"
                    placeholder="cliente@correo.com"
                    maxLength={CORREO_MAX}
                    key={form.key(CLIENTE_FIELDS.CORREO)}
                    {...form.getInputProps(CLIENTE_FIELDS.CORREO)}
                />
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 6 }}>
                <TextInput
                    label="Teléfono"
                    placeholder="+506 8888-8888"
                    maxLength={TELEFONO_MAX}
                    key={form.key(CLIENTE_FIELDS.TELEFONO)}
                    {...form.getInputProps(CLIENTE_FIELDS.TELEFONO)}
                />
            </Grid.Col>
            <Grid.Col span={12}>
                <Textarea
                    label="Observaciones"
                    placeholder="Notas internas del cliente"
                    autosize
                    minRows={4}
                    maxLength={OBSERVACIONES_MAX}
                    key={form.key(CLIENTE_FIELDS.OBSERVACIONES)}
                    {...form.getInputProps(CLIENTE_FIELDS.OBSERVACIONES)}
                />
            </Grid.Col>
        </Grid>
    );
}
