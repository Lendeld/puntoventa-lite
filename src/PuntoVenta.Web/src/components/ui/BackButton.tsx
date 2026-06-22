"use client";

import { Button } from "@mantine/core";
import { IconArrowLeft } from "@tabler/icons-react";
import Link from "next/link";

interface Props {
    href: string;
    label: string;
}

export function BackButton({ href, label }: Props) {
    return (
        <Button
            component={Link}
            href={href}
            variant="light"
            size="xs"
            leftSection={<IconArrowLeft size={14} />}
            w="fit-content"
        >
            {label}
        </Button>
    );
}
