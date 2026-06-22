"use client";

import { notifications } from "@mantine/notifications";
import {
    IconAlertTriangle,
    IconCheck,
    IconInfoCircle,
    IconX,
} from "@tabler/icons-react";
import type { ReactNode } from "react";

type NotificationVariant = "success" | "error" | "warning" | "info";

interface AppNotificationOptions {
    title?: ReactNode;
    message?: ReactNode;
    autoClose?: number | false;
}

const DEFAULT_CONTENT: Record<
    NotificationVariant,
    { title: string; message: string; color: string; icon: ReactNode }
> = {
    success: {
        title: "Operación exitosa",
        message: "Los cambios se guardaron correctamente.",
        color: "accentPV",
        icon: <IconCheck size={18} />,
    },
    error: {
        title: "Ocurrió un error",
        message: "No fue posible completar la operación.",
        color: "red",
        icon: <IconX size={18} />,
    },
    warning: {
        title: "Atención",
        message: "Revise la información antes de continuar.",
        color: "yellow",
        icon: <IconAlertTriangle size={18} />,
    },
    info: {
        title: "Información",
        message: "La operación se completó.",
        color: "blue",
        icon: <IconInfoCircle size={18} />,
    },
};

function showNotification(
    variant: NotificationVariant,
    options: AppNotificationOptions = {},
) {
    const current = DEFAULT_CONTENT[variant];

    notifications.show({
        color: current.color,
        title: options.title ?? current.title,
        message: options.message ?? current.message,
        icon: current.icon,
        autoClose: options.autoClose ?? 4000,
        classNames: {
            root: "bg-theme-surface border border-theme-border-soft shadow-lg",
            title: "text-theme-text font-semibold",
            description: "text-theme-text-muted",
            closeButton: "text-theme-text-muted hover:text-theme-text",
        },
    });
}

export const AppNotifier = {
    success: (options?: AppNotificationOptions) =>
        showNotification("success", options),
    error: (options?: AppNotificationOptions) =>
        showNotification("error", options),
    warning: (options?: AppNotificationOptions) =>
        showNotification("warning", options),
    info: (options?: AppNotificationOptions) =>
        showNotification("info", options),
};
