"use client";

import { RoleSelect } from "@/components/ui/selects/RoleSelect";
import { AppNotifier } from "@components/ui/AppNotifier";
import { crearUsuarioAction } from "@lib/actions/usuarios.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { crearUsuarioSchema } from "@lib/schemas/usuarios.schema";
import type { CrearUsuarioFormValues } from "@lib/types/usuarios.types";
import { generarPasswordTemporal } from "@lib/utils/password.utils";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    Grid,
    Group,
    PasswordInput,
    Text,
    TextInput,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import { IconAlertCircle, IconCopy, IconRefresh } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

export function NewUsuarioForm() {
    const queryClient = useQueryClient();

    const form = useForm<CrearUsuarioFormValues>({
        initialValues: {
            [USUARIO_FIELDS.NOMBRE_USUARIO]: "",
            [USUARIO_FIELDS.NOMBRE]: "",
            [USUARIO_FIELDS.IDENTIFICACION]: "",
            [USUARIO_FIELDS.PASSWORD]: "",
            [USUARIO_FIELDS.ROL_ID]: "",
            [USUARIO_FIELDS.CORREO]: "",
            [USUARIO_FIELDS.TELEFONO]: "",
        },
        validate: zodResolver(crearUsuarioSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CrearUsuarioFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.usuarios.all,
                });
                AppNotifier.success({
                    message:
                        "Usuario creado exitosamente. Deberá cambiar su contraseña al primer ingreso.",
                });
                modals.closeAll();
            },
        });

    const [passwordVisible, setPasswordVisible] = useState(false);

    function handleGenerarPassword() {
        const generada = generarPasswordTemporal();
        form.setFieldValue(USUARIO_FIELDS.PASSWORD, generada);
        form.clearFieldError(USUARIO_FIELDS.PASSWORD);
        setPasswordVisible(true);
    }

    async function handleCopiarPassword() {
        const actual = form.values[USUARIO_FIELDS.PASSWORD];
        if (!actual) {
            AppNotifier.warning({ message: "No hay contraseña para copiar." });
            return;
        }
        try {
            await navigator.clipboard.writeText(actual);
            AppNotifier.success({ message: "Contraseña copiada al portapapeles." });
        } catch {
            AppNotifier.error({ message: "No se pudo copiar la contraseña." });
        }
    }

    async function handleSubmit(values: CrearUsuarioFormValues) {
        // crearUsuarioAction devuelve ActionResult — compatible con execute()
        await execute(() => crearUsuarioAction(values));
    }

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Grid gap="md">
                {error && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        variant="light"
                        color="red"
                    >
                        {error}
                    </Alert>
                )}

                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                        label="Nombre de usuario"
                        placeholder="nombre.usuario"
                        required
                        maxLength={50}
                        key={form.key(USUARIO_FIELDS.NOMBRE_USUARIO)}
                        {...form.getInputProps(USUARIO_FIELDS.NOMBRE_USUARIO)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                        label="Nombre completo"
                        placeholder="Nombre completo"
                        required
                        maxLength={150}
                        key={form.key(USUARIO_FIELDS.NOMBRE)}
                        {...form.getInputProps(USUARIO_FIELDS.NOMBRE)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                        label="Identificación"
                        placeholder="Número de identificación"
                        maxLength={50}
                        key={form.key(USUARIO_FIELDS.IDENTIFICACION)}
                        {...form.getInputProps(USUARIO_FIELDS.IDENTIFICACION)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <RoleSelect
                        label="Rol"
                        placeholder="Selecciona un rol (opcional)"
                        clearable
                        key={form.key(USUARIO_FIELDS.ROL_ID)}
                        {...form.getInputProps(USUARIO_FIELDS.ROL_ID)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                        label="Correo"
                        placeholder="correo@ejemplo.com"
                        maxLength={256}
                        key={form.key(USUARIO_FIELDS.CORREO)}
                        {...form.getInputProps(USUARIO_FIELDS.CORREO)}
                    />
                </Grid.Col>
                <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                        label="Teléfono"
                        placeholder="+506 0000 0000"
                        maxLength={20}
                        key={form.key(USUARIO_FIELDS.TELEFONO)}
                        {...form.getInputProps(USUARIO_FIELDS.TELEFONO)}
                    />
                </Grid.Col>
                <Grid.Col span={12}>
                    <PasswordInput
                        label="Contraseña temporal"
                        placeholder="Mín. 8 caracteres con mayúscula, minúscula, número y símbolo"
                        required
                        maxLength={100}
                        visible={passwordVisible}
                        onVisibilityChange={setPasswordVisible}
                        key={form.key(USUARIO_FIELDS.PASSWORD)}
                        {...form.getInputProps(USUARIO_FIELDS.PASSWORD)}
                    />
                    <Group justify="space-between" gap="xs" mt={6}>
                        <Text size="xs" c="dimmed">
                            El usuario deberá cambiarla al primer ingreso.
                        </Text>
                        <Group gap="xs">
                            <Button
                                size="compact-xs"
                                variant="subtle"
                                leftSection={<IconRefresh size={14} />}
                                onClick={handleGenerarPassword}
                            >
                                Generar
                            </Button>
                            <Button
                                size="compact-xs"
                                variant="subtle"
                                leftSection={<IconCopy size={14} />}
                                onClick={() => void handleCopiarPassword()}
                            >
                                Copiar
                            </Button>
                        </Group>
                    </Group>
                </Grid.Col>
                <Button type="submit" loading={loading} fullWidth mt="xs">
                    Crear usuario
                </Button>
            </Grid>
        </form>
    );
}
