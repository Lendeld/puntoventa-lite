"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import {
    actualizarUsuarioActualAction,
    cambiarPasswordUsuarioActualAction,
    establecerPinUsuarioActualAction,
} from "@lib/actions/auth.actions";
import {
    CAMBIAR_PASSWORD_FIELDS,
    ESTABLECER_PIN_FIELDS,
} from "@lib/constants/auth.constants";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import {
    actualizarPerfilUsuarioActualSchema,
    cambiarPasswordSchema,
    establecerPinSchema,
} from "@lib/schemas/auth.schema";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import type {
    ActualizarPerfilUsuarioActualFormValues,
    CambiarPasswordFormValues,
    EstablecerPinFormValues,
    UsuarioActualDto,
} from "@lib/types/auth.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Badge,
    Box,
    Button,
    Grid,
    Group,
    Loader,
    Paper,
    PasswordInput,
    PinInput,
    Stack,
    Tabs,
    Text,
    TextInput,
} from "@mantine/core";
import { PasswordStrengthInput } from "@ui/PasswordStrengthInput";
import { useMediaQuery } from "@mantine/hooks";
import { useForm } from "@mantine/form";
import {
    IconAlertCircle,
    IconAt,
    IconFingerprint,
    IconId,
    IconKey,
    IconLockPassword,
    IconPhone,
    IconUser,
} from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { parseAsString, useQueryState } from "nuqs";
import { useEffect, useRef } from "react";

function MetaRow({
    label,
    value,
    hint,
}: {
    label: string;
    value: React.ReactNode;
    hint?: string;
}) {
    return (
        <Group justify="space-between" align="flex-start" wrap="nowrap">
            <Stack gap={2}>
                <Text size="sm" fw={700}>
                    {label}
                </Text>
                {hint && (
                    <Text size="xs" c="dimmed">
                        {hint}
                    </Text>
                )}
            </Stack>
            <Box
                style={{
                    display: "flex",
                    justifyContent: "flex-end",
                    alignItems: "center",
                    textAlign: "right",
                }}
            >
                {value}
            </Box>
        </Group>
    );
}

function SectionCard({
    icon,
    title,
    description,
    children,
}: {
    icon: React.ReactNode;
    title: string;
    description: string;
    children: React.ReactNode;
}) {
    return (
        <Paper
            withBorder
            radius="lg"
            className="bg-theme-surface overflow-hidden"
        >
            <Group
                align="flex-start"
                justify="space-between"
                wrap="wrap"
                gap="sm"
                className="w-full shrink-0 border-b border-theme p-5"
            >
                <Group align="flex-start" wrap="nowrap" gap="md" miw={0}>
                    {icon}
                    <Stack gap={2}>
                        <Text fw={700}>{title}</Text>
                        <Text size="sm" c="dimmed">
                            {description}
                        </Text>
                    </Stack>
                </Group>
            </Group>

            <Stack gap="md" p="lg">
                {children}
            </Stack>
        </Paper>
    );
}

// react-doctor-disable-next-line react-doctor/no-giant-component
export default function MiPerfilPageSection() {
    const esDesktop = useMediaQuery("(min-width: 62em)");
    const queryClient = useQueryClient();
    const hydratedUserIdRef = useRef<string | null>(null);

    const tabsValidos = ["cuenta", "contrasena", "pin"];
    const [activeTab, setActiveTab] = useQueryState(
        "tab",
        parseAsString
            .withDefault("cuenta")
            .withOptions({ scroll: false, throttleMs: 0 }),
    );
    const resolvedTab = tabsValidos.includes(activeTab) ? activeTab : "cuenta";

    const perfilForm = useForm<ActualizarPerfilUsuarioActualFormValues>({
        initialValues: {
            [USUARIO_FIELDS.NOMBRE]: "",
            [USUARIO_FIELDS.IDENTIFICACION]: "",
            [USUARIO_FIELDS.CORREO]: "",
            [USUARIO_FIELDS.TELEFONO]: "",
        },
        validate: zodResolver(actualizarPerfilUsuarioActualSchema),
    });

    const passwordForm = useForm<CambiarPasswordFormValues>({
        initialValues: {
            [CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL]: "",
            [CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA]: "",
            [CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA]: "",
        },
        validate: zodResolver(cambiarPasswordSchema),
    });

    const pinForm = useForm<EstablecerPinFormValues>({
        initialValues: {
            [ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL]: "",
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "",
        },
        validate: zodResolver(establecerPinSchema),
    });

    const {
        data: usuario,
        isLoading,
        isError,
    } = useQuery({
        queryKey: QUERY_KEYS.auth.usuarioActual,
        queryFn: async () => {
            const response = await obtenerUsuarioActualService();
            if (response.errors) {
                throw response.errors;
            }

            return response.data as UsuarioActualDto;
        },
        staleTime: 1000 * 60 * 5,
    });

    useEffect(() => {
        if (!usuario) return;
        if (hydratedUserIdRef.current === usuario.usuario) return;

        perfilForm.setValues({
            [USUARIO_FIELDS.NOMBRE]: usuario.nombre,
            [USUARIO_FIELDS.IDENTIFICACION]: usuario.identificacion,
            [USUARIO_FIELDS.CORREO]: usuario.correo ?? "",
            [USUARIO_FIELDS.TELEFONO]: usuario.telefono ?? "",
        });
        perfilForm.resetDirty();
        hydratedUserIdRef.current = usuario.usuario;
    }, [perfilForm, usuario]);

    const {
        execute: executePerfilUpdate,
        loading: perfilLoading,
        error: perfilError,
        setError: setPerfilError,
    } = useActionHandler<ActualizarPerfilUsuarioActualFormValues>({
        form: perfilForm,
        onSuccess: async () => {
            perfilForm.resetDirty(perfilForm.getValues());
            await queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.auth.usuarioActual,
            });
            AppNotifier.success({
                message: "Perfil actualizado exitosamente.",
            });
        },
    });

    const {
        execute: executePasswordUpdate,
        loading: passwordLoading,
        error: passwordError,
        setError: setPasswordError,
    } = useActionHandler<CambiarPasswordFormValues>({
        form: passwordForm,
        onSuccess: async () => {
            AppNotifier.success({
                message:
                    "Contraseña actualizada. Debes iniciar sesión otra vez.",
            });
            passwordForm.reset();
            window.location.replace(ROUTES.LOGIN);
        },
    });

    const {
        execute: executePinUpdate,
        loading: pinLoading,
        error: pinError,
        setError: setPinError,
    } = useActionHandler<EstablecerPinFormValues>({
        form: pinForm,
        onSuccess: async () => {
            AppNotifier.success({
                message: "PIN de seguridad actualizado exitosamente.",
            });
            pinForm.reset();
            await queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.auth.usuarioActual,
            });
        },
    });

    async function handlePerfilSubmit(
        values: ActualizarPerfilUsuarioActualFormValues,
    ) {
        await executePerfilUpdate(() => actualizarUsuarioActualAction(values));
    }

    async function handlePasswordSubmit(values: CambiarPasswordFormValues) {
        await executePasswordUpdate(() =>
            cambiarPasswordUsuarioActualAction(values),
        );
    }

    async function handlePinSubmit(values: EstablecerPinFormValues) {
        await executePinUpdate(() => establecerPinUsuarioActualAction(values));
    }

    if (isLoading) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isError || !usuario) {
        return (
            <Alert
                icon={<IconAlertCircle size={16} />}
                color="red"
                variant="light"
            >
                No fue posible cargar tu perfil.
            </Alert>
        );
    }

    return (
        <Stack gap="lg">
            <Tabs
                value={resolvedTab}
                onChange={(value) => setActiveTab(value ?? "cuenta")}
                variant="pills"
                radius="lg"
                keepMounted={false}
                orientation={esDesktop ? "vertical" : "horizontal"}
            >
                <Group align="flex-start" wrap="nowrap" w="100%">
                    <Box
                        className={esDesktop ? "shrink-0 w-60 mr-6" : "w-full"}
                    >
                        <Tabs.List grow={!esDesktop}>
                            <Tabs.Tab
                                value="cuenta"
                                leftSection={<IconUser size={16} />}
                            >
                                Cuenta
                            </Tabs.Tab>
                            <Tabs.Tab
                                value="contrasena"
                                leftSection={<IconLockPassword size={16} />}
                            >
                                Contraseña
                            </Tabs.Tab>
                            <Tabs.Tab
                                value="pin"
                                leftSection={<IconFingerprint size={16} />}
                            >
                                PIN
                            </Tabs.Tab>
                        </Tabs.List>
                    </Box>

                    <Stack flex={1} gap="md" w="100%" miw={0}>
                        <Tabs.Panel value="cuenta" pt={esDesktop ? 0 : "md"}>
                            <SectionCard
                                icon={<IconUser size={22} />}
                                title="Datos personales"
                                description="Actualiza la información básica de tu cuenta. Usuario e identificación se mantienen solo como referencia."
                            >
                                <Grid gap="md">
                                    <Grid.Col span={12}>
                                        <Paper withBorder radius="lg" p="md">
                                            <Stack gap="sm">
                                                <MetaRow
                                                    label="Cuenta"
                                                    value={usuario.nombre}
                                                    hint={
                                                        usuario.correo ||
                                                        "Sin correo registrado"
                                                    }
                                                />
                                                <MetaRow
                                                    label="Usuario"
                                                    value={`@${usuario.usuario}`}
                                                    hint="Nombre de acceso"
                                                />
                                            </Stack>
                                        </Paper>
                                    </Grid.Col>
                                </Grid>

                                {perfilError && (
                                    <Alert
                                        icon={<IconAlertCircle size={16} />}
                                        color="red"
                                        variant="light"
                                    >
                                        {perfilError}
                                    </Alert>
                                )}

                                <form
                                    onSubmit={perfilForm.onSubmit(
                                        handlePerfilSubmit,
                                        () => setPerfilError(null),
                                    )}
                                    noValidate
                                >
                                    <Stack gap="md">
                                        <TextInput
                                            label="Nombre completo"
                                            leftSection={<IconUser size={16} />}
                                            required
                                            key={perfilForm.key(
                                                USUARIO_FIELDS.NOMBRE,
                                            )}
                                            {...perfilForm.getInputProps(
                                                USUARIO_FIELDS.NOMBRE,
                                            )}
                                        />
                                        <TextInput
                                            label="Identificación"
                                            leftSection={<IconId size={16} />}
                                            key={perfilForm.key(
                                                USUARIO_FIELDS.IDENTIFICACION,
                                            )}
                                            {...perfilForm.getInputProps(
                                                USUARIO_FIELDS.IDENTIFICACION,
                                            )}
                                        />
                                        <TextInput
                                            label="Correo"
                                            leftSection={<IconAt size={16} />}
                                            key={perfilForm.key(
                                                USUARIO_FIELDS.CORREO,
                                            )}
                                            {...perfilForm.getInputProps(
                                                USUARIO_FIELDS.CORREO,
                                            )}
                                        />
                                        <TextInput
                                            label="Teléfono"
                                            leftSection={
                                                <IconPhone size={16} />
                                            }
                                            key={perfilForm.key(
                                                USUARIO_FIELDS.TELEFONO,
                                            )}
                                            {...perfilForm.getInputProps(
                                                USUARIO_FIELDS.TELEFONO,
                                            )}
                                        />
                                        <Button
                                            type="submit"
                                            loading={perfilLoading}
                                            disabled={!perfilForm.isDirty()}
                                            w="fit-content"
                                        >
                                            Guardar cambios
                                        </Button>
                                    </Stack>
                                </form>
                            </SectionCard>
                        </Tabs.Panel>

                        <Tabs.Panel
                            value="contrasena"
                            pt={esDesktop ? 0 : "md"}
                        >
                            <SectionCard
                                icon={<IconLockPassword size={22} />}
                                title="Contraseña"
                                description="Cambia tu contraseña desde aquí. Por seguridad, después del cambio tendrás que iniciar sesión nuevamente."
                            >
                                {passwordError && (
                                    <Alert
                                        icon={<IconAlertCircle size={16} />}
                                        color="red"
                                        variant="light"
                                    >
                                        {passwordError}
                                    </Alert>
                                )}

                                <form
                                    onSubmit={passwordForm.onSubmit(
                                        handlePasswordSubmit,
                                        () => setPasswordError(null),
                                    )}
                                    noValidate
                                >
                                    <Stack gap="md">
                                        <PasswordInput
                                            label="Contraseña actual"
                                            placeholder="Ingresa tu contraseña actual"
                                            leftSection={<IconKey size={16} />}
                                            key={passwordForm.key(
                                                CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL,
                                            )}
                                            {...passwordForm.getInputProps(
                                                CAMBIAR_PASSWORD_FIELDS.PASSWORD_ACTUAL,
                                            )}
                                        />
                                        <PasswordStrengthInput
                                            label="Nueva contraseña"
                                            placeholder="Crea una nueva contraseña"
                                            leftSection={<IconKey size={16} />}
                                            key={passwordForm.key(
                                                CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA,
                                            )}
                                            {...passwordForm.getInputProps(
                                                CAMBIAR_PASSWORD_FIELDS.PASSWORD_NUEVA,
                                            )}
                                        />
                                        <PasswordInput
                                            label="Confirmar nueva contraseña"
                                            placeholder="Repite la nueva contraseña"
                                            leftSection={<IconKey size={16} />}
                                            key={passwordForm.key(
                                                CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA,
                                            )}
                                            {...passwordForm.getInputProps(
                                                CAMBIAR_PASSWORD_FIELDS.CONFIRMAR_PASSWORD_NUEVA,
                                            )}
                                        />
                                        <Button
                                            type="submit"
                                            loading={passwordLoading}
                                            w="fit-content"
                                        >
                                            Actualizar contraseña
                                        </Button>
                                    </Stack>
                                </form>
                            </SectionCard>
                        </Tabs.Panel>

                        <Tabs.Panel value="pin" pt={esDesktop ? 0 : "md"}>
                            <SectionCard
                                icon={<IconFingerprint size={22} />}
                                title="PIN de seguridad"
                                description="El PIN protege acciones sensibles como backup y restauración. Usa exactamente 6 dígitos numéricos."
                            >
                                <Group gap="xs">
                                    <Text size="sm" c="dimmed">
                                        Estado actual:
                                    </Text>
                                    {usuario.tienePin ? (
                                        <Badge color="green" size="sm">
                                            Configurado
                                        </Badge>
                                    ) : (
                                        <Badge color="orange" size="sm">
                                            Sin configurar
                                        </Badge>
                                    )}
                                </Group>

                                {pinError && (
                                    <Alert
                                        icon={<IconAlertCircle size={16} />}
                                        color="red"
                                        variant="light"
                                    >
                                        {pinError}
                                    </Alert>
                                )}

                                <form
                                    onSubmit={pinForm.onSubmit(
                                        handlePinSubmit,
                                        () => setPinError(null),
                                    )}
                                    noValidate
                                >
                                    <Stack gap="md">
                                        <PasswordInput
                                            label="Contraseña actual"
                                            placeholder="Ingresa tu contraseña para confirmar"
                                            leftSection={<IconKey size={16} />}
                                            key={pinForm.key(
                                                ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL,
                                            )}
                                            {...pinForm.getInputProps(
                                                ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL,
                                            )}
                                        />
                                        <Box>
                                            <Text size="sm" fw={500} mb={6}>
                                                Nuevo PIN
                                            </Text>
                                            <PinInput
                                                length={6}
                                                type="number"
                                                mask
                                                key={pinForm.key(
                                                    ESTABLECER_PIN_FIELDS.PIN_NUEVO,
                                                )}
                                                {...pinForm.getInputProps(
                                                    ESTABLECER_PIN_FIELDS.PIN_NUEVO,
                                                )}
                                            />
                                            {pinForm.errors[ESTABLECER_PIN_FIELDS.PIN_NUEVO] && (
                                                <Text size="xs" c="red" mt={4}>
                                                    {pinForm.errors[ESTABLECER_PIN_FIELDS.PIN_NUEVO]}
                                                </Text>
                                            )}
                                        </Box>
                                        <Box>
                                            <Text size="sm" fw={500} mb={6}>
                                                Confirmar PIN
                                            </Text>
                                            <PinInput
                                                length={6}
                                                type="number"
                                                mask
                                                key={pinForm.key(
                                                    ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO,
                                                )}
                                                {...pinForm.getInputProps(
                                                    ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO,
                                                )}
                                            />
                                            {pinForm.errors[ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO] && (
                                                <Text size="xs" c="red" mt={4}>
                                                    {pinForm.errors[ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]}
                                                </Text>
                                            )}
                                        </Box>
                                        <Button
                                            type="submit"
                                            loading={pinLoading}
                                            w="fit-content"
                                        >
                                            {usuario.tienePin
                                                ? "Cambiar PIN"
                                                : "Configurar PIN"}
                                        </Button>
                                    </Stack>
                                </form>
                            </SectionCard>
                        </Tabs.Panel>
                    </Stack>
                </Group>
            </Tabs>
        </Stack>
    );
}
