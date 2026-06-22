"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewCajaForm } from "@pages/cajas/new/NewCajaForm";

export default function NewCajaSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nueva caja",
            centered: true,
            size: "md",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: { opacity: 1, blur: 3 },
            children: <NewCajaForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nueva caja
        </Button>
    );
}
