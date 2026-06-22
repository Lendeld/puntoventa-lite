"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { useActionHandler } from "@lib/hooks/useActionHandler";
import useConfirmModal from "@lib/hooks/useConfirmModal";
import type { ActionResult } from "@lib/types/base.types";
import { Switch } from "@mantine/core";

interface Props {
    activo: boolean;
    disabled?: boolean;
    onToggle: () => Promise<ActionResult>;
    onSuccess?: () => Promise<void> | void;
    successMessage?: string;
    confirmTitle: string;
    confirmMessage: string;
    confirmLabel: string;
    cancelLabel?: string;
    confirmVariant?: "danger" | "default";
}

export function EstadoActivoToggle({
    activo,
    disabled = false,
    onToggle,
    onSuccess,
    successMessage,
    confirmTitle,
    confirmMessage,
    confirmLabel,
    cancelLabel = "No, cancelar",
    confirmVariant,
}: Props) {
    const { execute, loading } = useActionHandler<
        Record<string, never>,
        ActionResult
    >({
        forbiddenMessage: "No tienes permiso para cambiar el estado.",
        onSuccess: async () => {
            await onSuccess?.();

            AppNotifier.success({
                message:
                    successMessage ??
                    `Estado actualizado a ${activo ? "inactivo" : "activo"}.`,
            });
        },
    });

    async function handleToggle() {
        if (disabled || loading) return;
        await execute(onToggle);
    }

    const openConfirmToggle = useConfirmModal({
        title: confirmTitle,
        message: confirmMessage,
        labels: {
            confirm: confirmLabel,
            cancel: cancelLabel,
        },
        variant: confirmVariant,
        overlay: true,
        onConfirm: () => {
            void handleToggle();
        },
    });

    return (
        <Switch
            checked={activo}
            onChange={() => openConfirmToggle()}
            disabled={disabled || loading}
            size="md"
            aria-label={activo ? "Activo" : "Inactivo"}
        />
    );
}
