"use client";

import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import { obtenerUsuarioActualService } from "@lib/services/auth.service";
import type { UsuarioActualDto } from "@lib/types/auth.types";
import { MenuItem } from "@components/ui/MenuItem";
import { Avatar, Group, Menu, Stack, Text } from "@mantine/core";
import {
    IconChevronRight,
    IconLogout,
    IconMail,
    IconShieldCog,
    IconUser,
} from "@tabler/icons-react";
import { useQuery } from "@tanstack/react-query";

function obtenerIniciales(usuario?: UsuarioActualDto) {
    const texto = usuario?.nombre?.trim() || usuario?.usuario?.trim() || "";
    if (!texto) return "PV";

    const partes = texto.split(/\s+/).filter(Boolean);
    return partes
        .slice(0, 2)
        .map((parte) => parte[0]?.toUpperCase() ?? "")
        .join("");
}

export default function MenuPerfil() {
    const {
        data,
        isLoading: cargandoUsuario,
        error: errorUsuario,
    } = useQuery({
        queryKey: QUERY_KEYS.auth.usuarioActual,
        queryFn: async () => {
            const response = await obtenerUsuarioActualService();
            if (response.errors) {
                throw new Error(
                    response.errors.title ||
                        "No fue posible obtener usuario actual.",
                );
            }

            return response.data!;
        },
        staleTime: 1000 * 60 * 5,
    });

    function handleCerrarSesion() {
        try {
            const channel = new BroadcastChannel("pv:auth");
            channel.postMessage({ type: "logout" });
            channel.close();
        } catch {
            // Ignore if BroadcastChannel not available.
        }

        window.location.replace(ROUTES.API_LOGOUT);
    }

    const usuario = data;
    const nombreMostrar = usuario?.nombre || "Mi perfil";
    const usuarioMostrar = usuario?.usuario || "Usuario";
    const correoMostrar = usuario?.correo || "Sin correo";

    return (
        <Menu width={260} position="bottom-end" shadow="md">
            <Menu.Target>
                <Avatar
                    color="accentPV"
                    size="sm"
                    className="cursor-pointer shadow"
                >
                    {obtenerIniciales(usuario)}
                </Avatar>
            </Menu.Target>

            <Menu.Dropdown>
                {errorUsuario && (
                    <Text size="xs" c="red" className="text-center m-3">
                        No se pudo cargar datos usuario.
                    </Text>
                )}
                {!errorUsuario && !cargandoUsuario && usuario && (
                    <>
                        <Menu.Label>Perfil</Menu.Label>
                        <Stack gap={10} px="sm" py="xs">
                            <Group gap="xs" wrap="nowrap">
                                <IconUser size={16} />
                                <Text size="sm" fw={600} truncate>
                                    {cargandoUsuario
                                        ? "Cargando..."
                                        : nombreMostrar}
                                </Text>
                            </Group>
                            <Group gap="xs" wrap="nowrap">
                                <IconShieldCog size={14} />
                                <Text size="xs" c="dimmed" truncate>
                                    {usuarioMostrar}
                                </Text>
                            </Group>
                            <Group gap="xs" wrap="nowrap">
                                <IconMail size={14} />
                                <Text size="xs" c="dimmed" truncate>
                                    {correoMostrar}
                                </Text>
                            </Group>
                        </Stack>
                    </>
                )}
                <Menu.Divider />
                {!errorUsuario && !cargandoUsuario && usuario && (
                    <Menu.Item
                        leftSection={<IconUser size={16} />}
                        rightSection={<IconChevronRight size={14} />}
                        component="a"
                        href={ROUTES.MI_PERFIL}
                    >
                        Mi perfil y seguridad
                    </Menu.Item>
                )}
                <Menu.Divider />
                <MenuItem
                    variant="danger"
                    leftSection={<IconLogout size={16} />}
                    onClick={handleCerrarSesion}
                >
                    Cerrar sesión
                </MenuItem>
            </Menu.Dropdown>
        </Menu>
    );
}
