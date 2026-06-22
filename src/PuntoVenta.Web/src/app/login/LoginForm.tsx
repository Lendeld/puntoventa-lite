"use client";

import type { LoginFormValues } from "@/lib/types/auth.types";
import {
    loginAction,
    type LoginActionResult,
} from "@lib/actions/auth.actions";
import { LOGIN_FIELDS } from "@lib/constants/auth.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { useRedirectIfAuthenticated } from "@lib/hooks/useRedirectIfAuthenticated";
import { loginSchema } from "@lib/schemas/auth.schema";
import {
    resolveLoginErrorMessage,
} from "@lib/utils/authErrors";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Box,
    Button,
    Flex,
    PasswordInput,
    Stack,
    Text,
    TextInput,
    Title,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import {
    IconAlertCircle,
    IconChevronRight,
    IconLock,
    IconUser,
} from "@tabler/icons-react";
import { ColorSchemeToggle } from "@ui/ColorSchemeToggle";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

// Login simple sin OTP; el backend de Lite no expone verify-otp.
export function LoginForm() {
    const { push } = useRouter();
    useRedirectIfAuthenticated();

    // Warm-up del API para evitar que el cold start golpee el primer login/refresh
    useEffect(() => {
        void fetch("/api/health").catch(() => undefined);
    }, []);

    const loginForm = useForm<LoginFormValues>({
        initialValues: {
            [LOGIN_FIELDS.NOMBRE_USUARIO]: "",
            [LOGIN_FIELDS.PASSWORD]: "",
        },
        validate: zodResolver(loginSchema),
    });

    const {
        execute: executeLogin,
        loading: loadingLogin,
        error: loginError,
        setError: setLoginError,
    } = useActionHandler<LoginFormValues, LoginActionResult>({
        form: loginForm,
        resolveErrorMessage: (result) =>
            resolveLoginErrorMessage(result.status, result.errors),
        onSuccess: (result) => {
            push(result.data?.redirectTo ?? "/");
        },
        onForbidden: (_result, errorMessage) => {
            setLoginError(errorMessage);
        },
        onUnauthorized: (_result, errorMessage) => {
            setLoginError(errorMessage);
        },
        onInternalError(_result, errorMessage) {
            setLoginError(errorMessage);
        },
    });

    async function handleSubmit(values: LoginFormValues) {
        await executeLogin(() => loginAction(values));
    }

    return (
        <Flex
            flex={1}
            align="center"
            justify="center"
            className="bg-theme-canvas relative p-8"
        >
            <Box className="pointer-events-none absolute inset-0 overflow-hidden">
                <Box className="absolute -top-24 -left-20 size-72 rounded-full bg-theme-accent-soft blur-3xl" />
                <Box className="absolute -bottom-24 -right-16 size-80 rounded-full bg-theme-secondary blur-3xl opacity-70 dark:opacity-30" />
            </Box>
            <Box className="absolute top-4 right-4">
                <ColorSchemeToggle />
            </Box>

            <Stack
                className="relative w-full max-w-sm rounded-lg border border-theme-border-soft bg-theme-surface p-8 shadow-modal dark:border-theme"
                gap="md"
            >
                <Stack gap={6} mb="md">
                    <Title
                        order={1}
                        className="font-display text-4xl text-theme-text"
                    >
                        Bienvenido de vuelta
                    </Title>
                    <Text size="sm" className="text-theme-text-muted">
                        Ingresa a tu panel para comenzar el día.
                    </Text>
                </Stack>

                {loginError && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        variant="light"
                        color="red"
                    >
                        {loginError}
                    </Alert>
                )}

                <form
                    onSubmit={loginForm.onSubmit(handleSubmit, () =>
                        setLoginError(null),
                    )}
                    noValidate
                >
                    <Stack gap="md">
                        <TextInput
                            label="Usuario"
                            placeholder="Nombre de usuario"
                            leftSection={<IconUser size={16} />}
                            size="md"
                            key={loginForm.key(LOGIN_FIELDS.NOMBRE_USUARIO)}
                            {...loginForm.getInputProps(
                                LOGIN_FIELDS.NOMBRE_USUARIO,
                            )}
                        />

                        <PasswordInput
                            label="Contraseña"
                            placeholder="Tu contraseña"
                            leftSection={<IconLock size={16} />}
                            size="md"
                            key={loginForm.key(LOGIN_FIELDS.PASSWORD)}
                            {...loginForm.getInputProps(
                                LOGIN_FIELDS.PASSWORD,
                            )}
                        />

                        <Button
                            type="submit"
                            size="md"
                            loading={loadingLogin}
                            fullWidth
                            mt="xs"
                            rightSection={
                                !loadingLogin ? (
                                    <IconChevronRight size={18} />
                                ) : null
                            }
                        >
                            Entrar al panel
                        </Button>
                    </Stack>
                </form>
            </Stack>
        </Flex>
    );
}
