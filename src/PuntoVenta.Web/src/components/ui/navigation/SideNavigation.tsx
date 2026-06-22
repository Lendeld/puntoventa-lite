"use client";
import { Box, Stack } from "@mantine/core";
import { useEffect, useMemo, useState } from "react";
import { usePathname } from "next/navigation";
import { NAVIGATION_MENU } from "@lib/constants/navigation.constants";
import { isLeafActive } from "@lib/utils/routes.utils";
import { NavItem } from "@ui/navigation/NavItem";
import { NavGroup } from "@ui/navigation/NavGroup";

export function SideNavigation() {
    const pathname = usePathname();

    const activeGroup = useMemo(
        () =>
            NAVIGATION_MENU.find(
                (item) =>
                    "items" in item &&
                    item.items.some((sub) => isLeafActive(pathname, sub)),
            )?.label ?? null,
        [pathname],
    );

    const [openGroup, setOpenGroup] = useState<string | null>(
        () => activeGroup,
    );

    // Al navegar a otra sección, abre su módulo (single-open cierra el previo).
    // El toggle manual sobreescribe hasta la próxima navegación.
    useEffect(() => {
        setOpenGroup(activeGroup);
    }, [activeGroup]);

    return (
        <Stack gap={0} className="h-full overflow-y-auto overflow-x-hidden pt-2">
            {NAVIGATION_MENU.map((item) => {
                if (!("items" in item)) {
                    return (
                        <Box key={item.label} className="px-1">
                            <NavItem item={item} />
                        </Box>
                    );
                }

                return (
                    <Box key={item.label} className="px-1 pt-1.5">
                        <NavGroup
                            group={item}
                            isOpen={openGroup === item.label}
                            onToggle={() =>
                                setOpenGroup((prev) =>
                                    prev === item.label ? null : item.label,
                                )
                            }
                        />
                    </Box>
                );
            })}
        </Stack>
    );
}
