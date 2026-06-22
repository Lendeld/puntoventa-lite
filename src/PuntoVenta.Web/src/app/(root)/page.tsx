import {
    Group,
    SimpleGrid,
    Stack,
    Text,
    ThemeIcon,
    Title,
} from "@mantine/core";
import {
    IconListDetails,
    IconReceipt2,
} from "@tabler/icons-react";
import { Metadata } from "next";
import Link from "next/link";
import { DashboardContenido } from "@/components/dashboard/DashboardContenido";

export const metadata: Metadata = {
    title: "Menu principal",
};

const QUICK_ACTIONS = [
    {
        label: "Facturación",
        href: "/emision/facturacion",
        icon: IconReceipt2,
    },
    {
        label: "Ventas",
        href: "/emision/ventas",
        icon: IconListDetails,
    },
] as const;

export default function PrincipalPage() {
    return (
        <Stack gap="xl" className="mx-auto max-w-295 px-1 py-5 sm:px-0">
            <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="xl">
                <Stack gap="md" justify="center" className="min-w-0">
                    <Stack gap={6}>
                        <Text
                            component="span"
                            className="text-[11px] font-semibold uppercase text-theme-text-dim"
                        >
                            Panel principal
                        </Text>
                        <Title className="font-display text-4xl text-theme-text">
                            Pulso del negocio
                        </Title>
                    </Stack>
                    <Text size="sm" className="max-w-2xl text-theme-text-muted">
                        Facturación y ventas del día en una sola vista para
                        seguir el pulso del negocio sin salir del flujo.
                    </Text>
                </Stack>

                <Stack
                    gap="sm"
                    className="rounded-lg border border-theme-border-soft bg-[oklch(96.5%_0.012_255)] p-4 shadow-xs dark:bg-[oklch(19%_0.013_255)]"
                >
                    <Group justify="space-between" align="center" wrap="nowrap">
                        <Text
                            component="span"
                            className="text-[11px] font-semibold uppercase text-theme-text-dim"
                        >
                            Acciones rápidas
                        </Text>
                        <Text size="xs" className="text-theme-text-muted">
                            Operación diaria
                        </Text>
                    </Group>
                    <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="xs">
                        {QUICK_ACTIONS.map((action) => (
                            <Link
                                key={action.href}
                                href={action.href}
                                className="flex min-h-11 items-center gap-2 rounded-md border border-theme-border-soft bg-theme-surface px-3 py-2 text-sm font-semibold text-theme-text shadow-xs transition-[background-color,box-shadow,transform] duration-(--dur-fast) ease-soft hover:-translate-y-px hover:bg-theme-surface-2 hover:shadow-soft focus-visible:shadow-(--ring-accent) focus-visible:outline-none"
                            >
                                <span className="shrink-0">
                                    <ThemeIcon
                                        size="sm"
                                        radius="sm"
                                        variant="light"
                                    >
                                        <action.icon size={15} />
                                    </ThemeIcon>
                                </span>
                                <span className="leading-tight">{action.label}</span>
                            </Link>
                        ))}
                    </SimpleGrid>
                </Stack>
            </SimpleGrid>

            <DashboardContenido />
        </Stack>
    );
}
