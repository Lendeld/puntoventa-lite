"use client";

import { toggleEstadoCajaAction } from "@lib/actions/cajas.actions";
import { QUERY_KEYS } from "@lib/constants/queryKeys.constants";
import { AppNotifier } from "@components/ui/AppNotifier";
import { Menu, Text } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconToggleLeft, IconToggleRight } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";

interface Props {
    id: string;
    activo: boolean;
}

export default function MenuToggleCajaAccion({ id, activo }: Props) {
    const queryClient = useQueryClient();

    function handleToggle() {
        modals.openConfirmModal({
            title: activo ? "Desactivar caja" : "Activar caja",
            centered: true,
            overlayProps: { blur: 3, opacity: 1 },
            children: (
                <Text size="sm">
                    {activo
                        ? "¿Desactivar esta caja? No podrá ser usada en nuevas operaciones."
                        : "¿Activar esta caja?"}
                </Text>
            ),
            labels: {
                confirm: activo ? "Desactivar" : "Activar",
                cancel: "Cancelar",
            },
            confirmProps: { color: activo ? "red" : "accentPV" },
            cancelProps: { variant: "outline" },
            onConfirm: async () => {
                const result = await toggleEstadoCajaAction(id);
                if (result.errors) {
                    AppNotifier.error({
                        message: result.errors[Object.keys(result.errors)[0]] ?? "Error al cambiar el estado.",
                    });
                    return;
                }
                await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.cajas.all });
                AppNotifier.success({
                    message: activo
                        ? "Caja desactivada exitosamente."
                        : "Caja activada exitosamente.",
                });
            },
        });
    }

    return (
        <Menu.Item
            leftSection={
                activo ? <IconToggleLeft size={16} /> : <IconToggleRight size={16} />
            }
            color={activo ? "red" : undefined}
            onClick={handleToggle}
        >
            {activo ? "Desactivar" : "Activar"}
        </Menu.Item>
    );
}
