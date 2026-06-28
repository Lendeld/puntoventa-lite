"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewProveedorForm } from "@pages/mantenimiento/proveedores/NewProveedorForm";

export default function NewProveedorSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nuevo proveedor",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <NewProveedorForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nuevo Proveedor
        </Button>
    );
}
