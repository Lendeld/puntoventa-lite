import { Menu } from "@mantine/core";
import type { MenuItemProps as MantineMenuItemProps } from "@mantine/core";
import type React from "react";

type MenuItemVariant = "default" | "danger";

const variantStyles: Record<MenuItemVariant, React.CSSProperties> = {
    default: {},
    danger: {
        "--color-theme-text": "var(--mantine-color-white)",
        "--color-theme-accent-soft":
            "color-mix(in srgb, var(--color-theme-danger) 80%, black)",
        "--color-theme-surface-3":
            "color-mix(in srgb, var(--color-theme-danger) 80%, black)",
        backgroundColor: "var(--color-theme-danger)",
    } as React.CSSProperties,
};

interface MenuItemProps extends MantineMenuItemProps {
    variant?: MenuItemVariant;
    onClick?: React.MouseEventHandler<HTMLButtonElement>;
}

export function MenuItem({
    variant = "default",
    style,
    ...props
}: MenuItemProps) {
    const combined = { ...variantStyles[variant], ...style };
    return <Menu.Item {...props} style={combined} variant="" />;
}
