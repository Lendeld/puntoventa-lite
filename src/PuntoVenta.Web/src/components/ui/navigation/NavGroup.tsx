"use client";
import { useId } from "react";
import { Box, Group, Stack, Text, UnstyledButton } from "@mantine/core";
import { IconChevronRight } from "@tabler/icons-react";
import { usePathname } from "next/navigation";
import { AnimatePresence, m } from "framer-motion";
import { NavGroup as NavGroupType } from "@lib/constants/navigation.constants";
import { isLeafActive } from "@lib/utils/routes.utils";
import { expandCollapse, easeSoft } from "@ui/motion/MotionConfig";
import {
    NavItem,
    baseRow,
    activeClass,
    inactiveClass,
} from "@ui/navigation/NavItem";

interface Props {
    group: NavGroupType;
    isOpen: boolean;
    onToggle: () => void;
}

export function NavGroup({ group, isOpen, onToggle }: Props) {
    const pathname = usePathname();
    const panelId = useId();
    const hasActiveChild = group.items.some((item) =>
        isLeafActive(pathname, item),
    );

    const headerClass = `${baseRow} ${hasActiveChild ? activeClass : inactiveClass}`;

    return (
        <Box>
            <UnstyledButton
                className={headerClass}
                onClick={onToggle}
                aria-expanded={isOpen}
                aria-controls={panelId}
                w="100%"
            >
                <Group
                    justify="space-between"
                    wrap="nowrap"
                    gap={4}
                    className="w-full"
                >
                    <Text
                        component="span"
                        fw={600}
                        className={`truncate text-2xs uppercase tracking-wide ${
                            hasActiveChild ? "" : "text-theme-text-dim"
                        }`}
                    >
                        {group.label}
                    </Text>
                    <m.span
                        animate={{ rotate: isOpen ? 90 : 0 }}
                        transition={{ duration: 0.18, ease: easeSoft }}
                        className="inline-flex shrink-0"
                    >
                        <IconChevronRight size={14} />
                    </m.span>
                </Group>
            </UnstyledButton>
            <AnimatePresence initial={false}>
                {isOpen && (
                    <m.div
                        id={panelId}
                        key="panel"
                        variants={expandCollapse}
                        initial="initial"
                        animate="enter"
                        exit="exit"
                        className="overflow-hidden"
                    >
                        <Stack gap={2} className="pt-1 pb-2 pl-2">
                            {group.items.map((item) => (
                                <NavItem key={item.label} item={item} />
                            ))}
                        </Stack>
                    </m.div>
                )}
            </AnimatePresence>
        </Box>
    );
}
