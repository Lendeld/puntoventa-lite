import { Group } from "@mantine/core";
import { ReactNode } from "react";

interface Props {
    children: ReactNode;
}

export function TableHeader({ children }: Props) {
    return (
        <Group className="px-4 py-5 border-b border-theme bg-theme-surface">
            {children}
        </Group>
    );
}
