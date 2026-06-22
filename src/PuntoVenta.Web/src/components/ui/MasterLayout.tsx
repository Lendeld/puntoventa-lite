"use client";
import { ColorSchemeToggle } from "@ui/ColorSchemeToggle";
import { AppShell, Burger, Divider, Group, Text } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { usePathname } from "next/navigation";
import { AnimatePresence, LazyMotion, domAnimation, m } from "framer-motion";
import MenuPerfil from "@/components/ui/MenuPerfil";
import { SideNavigation } from "@ui/navigation/SideNavigation";
import { Breadcrumb } from "@ui/navigation/Breadcrumb";
import { PageTransition } from "@ui/motion/PageTransition";
import { sidebarReveal } from "@ui/motion/MotionConfig";
import { getBreadcrumbs } from "@lib/utils/routes.utils";
import { BREADCRUMB_ROUTES } from "@lib/constants/routes.constants";

interface Props {
    children: React.ReactNode;
}

export function MasterLayout({ children }: Props) {
    const [opened, { toggle }] = useDisclosure(true);
    const pathname = usePathname();
    const hasBreadcrumb =
        getBreadcrumbs(pathname, BREADCRUMB_ROUTES).length > 0;

    return (
        <LazyMotion features={domAnimation}>
        <AppShell
            padding="md"
            header={{ height: 48 }}
            navbar={{
                width: 232,
                breakpoint: "sm",
                collapsed: { desktop: !opened, mobile: !opened },
            }}
        >
            <AppShell.Header className="border-b border-theme-border-soft bg-white/80 backdrop-blur-md dark:bg-theme-secondary-dark/85 dark:border-b-theme">
                <Group justify="space-between" px="md" h="100%">
                    <Group>
                        <Burger opened={opened} onClick={toggle} size="sm" />
                        <Group gap="xs">
                            {/* eslint-disable-next-line @next/next/no-img-element */}
                            <img
                                src="/icons/logo.svg"
                                alt="Punto Venta Lite"
                                width={24}
                                height={24}
                                className="size-6 shrink-0"
                            />
                            <Text
                                component="span"
                                className="font-semibold tracking-tight text-theme-text"
                            >
                                Punto Venta Lite
                            </Text>
                        </Group>
                    </Group>

                    <Group gap="xs">
                        <ColorSchemeToggle />
                        <Divider
                            orientation="vertical"
                            size="sm"
                            className="h-8"
                        />
                        <MenuPerfil />
                    </Group>
                </Group>
            </AppShell.Header>
            <AppShell.Navbar
                px="md"
                py="sm"
                className="border-r border-theme-border-soft bg-white dark:bg-theme-secondary-dark dark:border-r-theme"
            >
                <AnimatePresence mode="wait" initial={false}>
                    {opened && (
                        <m.div
                            key="navbar"
                            variants={sidebarReveal}
                            initial="initial"
                            animate="enter"
                            exit="exit"
                            className="h-full"
                        >
                            <SideNavigation />
                        </m.div>
                    )}
                </AnimatePresence>
            </AppShell.Navbar>
            <AppShell.Main className={hasBreadcrumb ? "pt-26" : undefined}>
                <Breadcrumb navbarOpen={opened} />
                <PageTransition>{children}</PageTransition>
            </AppShell.Main>
        </AppShell>
        </LazyMotion>
    );
}
