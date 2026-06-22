"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewClienteForm } from "@pages/clientes/NewClienteForm";

export default function NewClienteSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nuevo cliente",
            centered: true,
            size: "xl",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <NewClienteForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nuevo Cliente
        </Button>
    );
}
