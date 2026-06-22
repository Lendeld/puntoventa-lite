"use client";

import { RoleSelect } from "@/components/ui/selects/RoleSelect";
import { AppNotifier } from "@components/ui/AppNotifier";
import { actualizarUsuarioAction } from "@lib/actions/usuarios.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { USUARIO_FIELDS } from "@lib/constants/usuarios.constants";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import { actualizarUsuarioSchema } from "@lib/schemas/usuarios.schema";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import { obtenerUsuarioPorIdService } from "@lib/services/usuarios.service";
import type { ActualizarUsuarioFormValues } from "@lib/types/usuarios.types";
import { zodResolver } from "@lib/utils/zodResolver";
import {
    Alert,
    Avatar,
    Badge,
    Button,
    Checkbox,
    Divider,
    Group,
    Loader,
    Stack,
    Text,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { modals } from "@mantine/modals";
import {
    IconAlertCircle,
    IconId,
    IconMail,
    IconPhone,
    IconShieldLock,
} from "@tabler/icons-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";

interface Props {
    id: string;
}

function obtenerIniciales(nombre: string): string {
    const partes = nombre.trim().split(/\s+/).filter(Boolean);
    if (partes.length === 0) return "?";
    if (partes.length === 1) return partes[0].charAt(0).toUpperCase();
    return (partes[0].charAt(0) + partes[1].charAt(0)).toUpperCase();
}

function MetaItem({
    icon,
    value,
}: {
    icon: React.ReactNode;
    value: string;
}) {
    return (
        <Group gap={6} wrap="nowrap">
            <span className="text-theme-text-dim flex items-center">{icon}</span>
            <Text size="sm" c="dimmed">
                {value}
            </Text>
        </Group>
    );
}

export default function EditUsuarioForm({ id }: Props) {
    const queryClient = useQueryClient();
    const hydratedUserIdRef = useRef<string | null>(null);
    const form = useForm<ActualizarUsuarioFormValues>({
        initialValues: {
            [USUARIO_FIELDS.ROL_ID]: "",
            [USUARIO_FIELDS.ACTIVO]: true,
        },
        validate: zodResolver(actualizarUsuarioSchema),
    });

    const {
        data: usuario,
        isLoading: loadingUsuario,
        isError: isUsuarioError,
    } = useQuery({
        queryKey: QUERY_KEYS.usuarios.detalle(id),
        queryFn: async () => {
            const res = await obtenerUsuarioPorIdService(id);
            if (res.errors) throw res.errors;
            return res.data!;
        },
    });

    const { data: usuarioActual } = useQuery({
        queryKey: QUERY_KEYS.auth.usuarioActual,
        queryFn: async () => {
            const res = await obtenerUsuarioActualService();
            if (res.errors) throw res.errors;
            return res.data!;
        },
        staleTime: 1000 * 60 * 5,
    });

    useEffect(() => {
        if (!usuario) return;
        if (hydratedUserIdRef.current === usuario.id) return;

        form.setValues({
            [USUARIO_FIELDS.ROL_ID]: usuario.rolId ?? "",
            [USUARIO_FIELDS.ACTIVO]: usuario.activo,
        });
        form.resetDirty();
        hydratedUserIdRef.current = usuario.id;
    // form.setValues/resetDirty de Mantine no son estables; deps solo [usuario]
    // es correcto (corre al cargar la data). Ver memoria mantine-form-methods.
    // react-doctor-disable-next-line react-doctor/exhaustive-deps
    }, [usuario]);

    const { execute, loading, error, setError } =
        useActionHandler<ActualizarUsuarioFormValues>({
            form,
            onSuccess: async () => {
                await queryClient.invalidateQueries({
                    queryKey: QUERY_KEYS.usuarios.all,
                });
                AppNotifier.success({
                    message: "Usuario actualizado exitosamente.",
                });
                modals.closeAll();
            },
        });

    async function handleSubmit(values: ActualizarUsuarioFormValues) {
        await execute(() => actualizarUsuarioAction(id, values));
    }

    if (loadingUsuario) {
        return (
            <Stack align="center" py="xl">
                <Loader color="accentPV" />
            </Stack>
        );
    }

    if (isUsuarioError || !usuario) {
        return (
            <Alert
                icon={<IconAlertCircle size={16} />}
                variant="light"
                color="red"
            >
                Error al cargar datos del usuario.
            </Alert>
        );
    }

    const esPropietario = usuario.esPropietario;
    // El propietario solo se edita a sí mismo; otros lo ven en solo lectura.
    const esPropietarioAjeno =
        esPropietario && usuarioActual?.usuario !== usuario.nombreUsuario;

    return (
        <form
            onSubmit={form.onSubmit(handleSubmit, () => setError(null))}
            noValidate
        >
            <Stack gap="lg">
                {error && (
                    <Alert
                        icon={<IconAlertCircle size={16} />}
                        variant="light"
                        color="red"
                    >
                        {error}
                    </Alert>
                )}

                <Group justify="space-between" align="flex-start" wrap="nowrap">
                    <Group gap="md" wrap="nowrap">
                        <Avatar color="accentPV" size="lg" radius="xl">
                            {obtenerIniciales(usuario.nombre)}
                        </Avatar>
                        <Stack gap={2}>
                            <Text fw={700} size="md" lh={1.2}>
                                {usuario.nombre}
                            </Text>
                            <Text size="sm" c="dimmed">
                                @{usuario.nombreUsuario}
                            </Text>
                        </Stack>
                    </Group>
                    <Group gap={6}>
                        {esPropietario && (
                            <Badge color="blue" variant="light">
                                Propietario
                            </Badge>
                        )}
                        <Badge
                            color={usuario.activo ? "green" : "red"}
                            variant="light"
                        >
                            {usuario.activo ? "Activo" : "Inactivo"}
                        </Badge>
                    </Group>
                </Group>

                <Group gap="lg" wrap="wrap">
                    <MetaItem
                        icon={<IconMail size={14} />}
                        value={usuario.correo ?? "Sin correo"}
                    />
                    <MetaItem
                        icon={<IconPhone size={14} />}
                        value={usuario.telefono ?? "Sin teléfono"}
                    />
                    <MetaItem
                        icon={<IconId size={14} />}
                        value={usuario.identificacion}
                    />
                </Group>

                <Divider />

                <Stack gap={2}>
                    <Text fw={600} size="sm">
                        Permisos y acceso
                    </Text>
                    <Text size="xs" c="dimmed">
                        Administra el rol y si el usuario puede seguir usando
                        este negocio.
                    </Text>
                </Stack>

                {esPropietarioAjeno && (
                    <Alert
                        icon={<IconShieldLock size={16} />}
                        variant="light"
                        color="gray"
                    >
                        Esta es la cuenta del propietario. Solo el propietario
                        puede modificarla; aquí la ves en solo lectura.
                    </Alert>
                )}

                <Stack gap="sm">
                    <RoleSelect
                        label="Rol"
                        placeholder="Selecciona un rol"
                        required
                        disabled={esPropietario}
                        key={form.key(USUARIO_FIELDS.ROL_ID)}
                        {...form.getInputProps(USUARIO_FIELDS.ROL_ID)}
                    />
                    {esPropietario && (
                        <Group gap={6} c="dimmed">
                            <IconShieldLock size={14} />
                            <Text size="xs" c="dimmed">
                                El rol del propietario no se puede modificar.
                            </Text>
                        </Group>
                    )}

                    <Checkbox
                        label="Usuario activo"
                        description={
                            esPropietarioAjeno
                                ? "Solo el propietario puede gestionar su propia cuenta."
                                : esPropietario
                                  ? "El propietario no puede desactivar su propia cuenta."
                                  : undefined
                        }
                        disabled={esPropietario}
                        key={form.key(USUARIO_FIELDS.ACTIVO)}
                        {...form.getInputProps(USUARIO_FIELDS.ACTIVO, {
                            type: "checkbox",
                        })}
                    />
                </Stack>

                <Divider />

                <Group justify="flex-end" gap="sm">
                    <Button
                        variant="light"
                        onClick={() => modals.closeAll()}
                        disabled={loading}
                    >
                        Cancelar
                    </Button>
                    <Button
                        type="submit"
                        loading={loading}
                        disabled={!form.isDirty() || esPropietarioAjeno}
                    >
                        Guardar cambios
                    </Button>
                </Group>
            </Stack>
        </form>
    );
}
