"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewVendedorForm } from "@pages/mantenimiento/vendedores/NewVendedorForm";

export default function NewVendedorSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nuevo vendedor",
            centered: true,
            size: "md",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <NewVendedorForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nuevo vendedor
        </Button>
    );
}
