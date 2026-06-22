"use client";

import { formatDate, type DateFormat } from "@lib/utils/date.utils";
import { HoverCard, Text } from "@mantine/core";

interface Props {
    date: Date | string | null | undefined;
    user?: string | null;
    title?: string;
    format?: DateFormat;
    emptyText?: string;
}

export function AuditDateHoverCard({
    date,
    user,
    title = "Usuario",
    format = "datetime-ampm",
    emptyText = "—",
}: Props) {
    if (!date) return emptyText;

    const formattedDate = formatDate(date, format);

    if (formattedDate === "—") return emptyText;

    return (
        <HoverCard shadow="md" withArrow openDelay={150}>
            <HoverCard.Target>
                <Text
                    component="span"
                    size="sm"
                    className="underline cursor-pointer inline-block w-fit"
                >
                    {formattedDate}
                </Text>
            </HoverCard.Target>
            <HoverCard.Dropdown>
                <Text size="xs" c="dimmed">
                    {title}
                </Text>
                <Text size="sm">{user?.trim() || "Sistema"}</Text>
            </HoverCard.Dropdown>
        </HoverCard>
    );
}
