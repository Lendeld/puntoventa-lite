"use client";
import { Badge, Group, Text, UnstyledButton } from "@mantine/core";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { m } from "framer-motion";
import { startNavigationProgress } from "@mantine/nprogress";
import { MenuItem } from "@lib/constants/navigation.constants";
import { isLeafActive } from "@lib/utils/routes.utils";
import { easeSoft } from "@ui/motion/MotionConfig";
import * as Icons from "@tabler/icons-react";

interface Props {
    item: MenuItem;
}

export const baseRow =
    "relative w-full flex items-center gap-2.5 px-2 py-1.5 rounded-md cursor-pointer transition-colors duration-(--dur-fast) ease-soft focus:outline-none focus-visible:outline-none focus-visible:shadow-(--ring-accent)";
export const activeClass =
    "bg-theme-accent-soft text-theme-text dark:text-theme-text/90";
export const inactiveClass =
    "text-theme-text-muted hover:bg-theme-surface-2 hover:text-theme-text";

export function NavItem({ item }: Props) {
    const pathname = usePathname();
    const IconComponent = Icons[item.icon as keyof typeof Icons] as
        | React.ComponentType<{ size?: number }>
        | undefined;

    const isActive = isLeafActive(pathname, item);
    const itemBadge = item.badge;

    const itemClass = `${baseRow} ${isActive ? activeClass : inactiveClass}`;

    return (
        <Link
            href={item.href}
            className="block"
            onClick={(e) => {
                if (item.href === pathname) {
                    e.preventDefault();
                    return;
                }
                startNavigationProgress();
            }}
        >
            <UnstyledButton className={itemClass} w="100%">
                {isActive && (
                    <m.span
                        layoutId="nav-active-indicator"
                        transition={{ duration: 0.22, ease: easeSoft }}
                        className="absolute left-1 top-1/2 -translate-y-1/2 h-4 w-0.5 rounded-full bg-theme/70"
                    />
                )}
                <Group
                    justify="space-between"
                    wrap="nowrap"
                    gap={4}
                    className="w-full"
                >
                    <Group gap={10} wrap="nowrap" className="flex-1 min-w-0">
                        {IconComponent && <IconComponent size={16} />}
                        <Text
                            size="sm"
                            className={`truncate ${isActive ? "font-semibold" : "font-medium"}`}
                        >
                            {item.label}
                        </Text>
                    </Group>
                    {itemBadge && (
                        <Badge
                            size="sm"
                            variant={isActive ? "filled" : "default"}
                            color="accentPV"
                        >
                            {itemBadge}
                        </Badge>
                    )}
                </Group>
            </UnstyledButton>
        </Link>
    );
}
