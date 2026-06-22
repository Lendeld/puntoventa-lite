"use client";

import { cambiarPasswordUsuarioActualAction, type LoginActionResult } from "@lib/actions/auth.actions";
import {
    CAMBIAR_PASSWORD_FIELDS,
} from "@lib/constants/auth.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { cambiarPasswordSchema } from "@lib/schemas/auth.schema";
import type { CambiarPasswordFormValues } from "@lib/types/auth.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Button,
    PasswordInput,
    Stack,
    Text,
    Title,
} from "@mantine/core";
import { PasswordStrengthInput } from "@ui/PasswordStrengthInput";
import { useForm } from "@mantine/form";
import { IconAlertCircle, IconKey } from "@tabler/icons-react";
import { useRouter } from "next/navigation";

export function CambiarPasswordObligatorioForm() {
    const { replace } = useRouter();
    const form = useForm<CambiarPasswordFormValues>({
        initialValues: {
            [CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL]: "",
            [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: "",
            [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: "",
        },
        validate: zodResolver(cambiarPasswordSchema),
    });

    const { execute, loading, error, setError } =
        useActionHandler<CambiarPasswordFormValues, LoginActionResult>({
            form,
            keepLoadingOnSuccess: true,
            onSuccess: (result) => {
                replace(result.data?.redirectTo ?? ROUTES.HOME);
            },
        });

    async function handleSubmit(values: CambiarPasswordFormValues) {
        await execute(() => cambiarPasswordUsuarioActualAction(values));
    }

    return (
        <Stack gap="lg">
            <Stack gap={4}>
                <Title order={1} size="h2">
                    Actualiza tu contraseña
                </Title>
                <Text c="dimmed" size="sm">
                    Antes de continuar, necesitas definir una nueva contraseña.
                </Text>
            </Stack>

            {error && (
                <Alert
                    icon={<IconAlertCircle size={16} />}
                    color="red"
                    variant="light"
                >
                    {error}
                </Alert>
            )}

            <form
                onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
                noValidate
            >
                <Stack gap="md">
                    <PasswordInput
                        label="Contraseña actual"
                        placeholder="Ingresa tu contraseña actual"
                        leftSection={<IconKey size={16} />}
                        key={form.key(CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL)}
                        {...form.getInputProps(
                            CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL,
                        )}
                    />

                    <PasswordStrengthInput
                        label="Nueva contraseña"
                        placeholder="Crea una nueva contraseña"
                        leftSection={<IconKey size={16} />}
                        key={form.key(CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA)}
                        {...form.getInputProps(
                            CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA,
                        )}
                    />

                    <PasswordInput
                        label="Confirmar nueva contraseña"
                        placeholder="Repite la nueva contraseña"
                        leftSection={<IconKey size={16} />}
                        key={form.key(
                            CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA,
                        )}
                        {...form.getInputProps(
                            CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA,
                        )}
                    />

                    <Button type="submit" loading={loading} fullWidth>
                        Guardar y continuar
                    </Button>
                </Stack>
            </form>
        </Stack>
    );
}
