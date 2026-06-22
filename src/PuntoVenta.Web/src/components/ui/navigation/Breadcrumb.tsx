"use client";
import { Breadcrumbs, Anchor, Text, Box, Group, Title } from "@mantine/core";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { BREADCRUMB_ROUTES } from "@lib/constants/routes.constants";
import { getBreadcrumbs } from "@lib/utils/routes.utils";

const NAVBAR_WIDTH = 232;
const HEADER_HEIGHT = 48;

interface Props {
    navbarOpen: boolean;
}

export function Breadcrumb({ navbarOpen }: Props) {
    const pathname = usePathname();
    const crumbs = getBreadcrumbs(pathname, BREADCRUMB_ROUTES);
    const isInventoryRoute = pathname.startsWith("/inventario");
    const inventoryParent = isInventoryRoute
        ? [
              {
                  pattern: "/inventario-root",
                  label: "Inventario",
                  href: null,
                  title_page: crumbs[crumbs.length - 1]?.title_page ?? "Inventario",
              },
          ]
        : [];
    const inventoryCrumbs =
        isInventoryRoute && pathname !== "/inventario" ? crumbs.slice(1) : crumbs;
    const breadcrumbItems = isInventoryRoute
        ? [...inventoryParent, ...inventoryCrumbs]
        : crumbs;

    if (breadcrumbItems.length === 0) return null;

    const showTrail = breadcrumbItems.length > 1;

    return (
        <Box
            style={{
                position: "fixed",
                top: HEADER_HEIGHT,
                left: navbarOpen ? NAVBAR_WIDTH : 0,
                right: 0,
                zIndex: 40,
                transition: "left 200ms ease",
            }}
            className="bg-white/85 backdrop-blur-md dark:bg-theme-secondary-dark/85 border-b border-theme-border-soft dark:border-theme px-4 py-2"
        >
            <Group justify="space-between" align="center">
                <Title
                    size="h3"
                    className="tracking-tight text-theme-text font-semibold"
                >
                    {breadcrumbItems[breadcrumbItems.length - 1].title_page}
                </Title>
                {showTrail && <Breadcrumbs
                    separator="/"
                    classNames={{
                        root: "text-sm",
                        separator: "text-theme-text-dim",
                    }}
                >
                    {breadcrumbItems.map((crumb, i) => {
                        const isLast = i === breadcrumbItems.length - 1;
                        if (isLast || !crumb.href) {
                            return (
                                <Text
                                    key={crumb.pattern}
                                    size="sm"
                                    className="text-theme-text font-medium"
                                >
                                    {crumb.label}
                                </Text>
                            );
                        }
                        return (
                            <Anchor
                                key={crumb.pattern}
                                component={Link}
                                href={crumb.href}
                                size="sm"
                                className="text-theme-text-muted hover:text-theme"
                            >
                                {crumb.label}
                            </Anchor>
                        );
                    })}
                </Breadcrumbs>}
            </Group>
        </Box>
    );
}
