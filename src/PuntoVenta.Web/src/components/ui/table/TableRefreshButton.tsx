"use client";

import { ActionIcon, Tooltip } from "@mantine/core";
import { IconRefresh } from "@tabler/icons-react";

interface Props {
    onClick: () => void;
    loading?: boolean;
    disabled?: boolean;
}

export function TableRefreshButton({
    onClick,
    loading = false,
    disabled = false,
}: Props) {
    return (
        <Tooltip label="Actualizar" withArrow>
            <ActionIcon
                variant="subtle"
                onClick={onClick}
                loading={loading}
                disabled={disabled}
                size="lg"
                aria-label="Actualizar tabla"
            >
                <IconRefresh size={18} />
            </ActionIcon>
        </Tooltip>
    );
}
