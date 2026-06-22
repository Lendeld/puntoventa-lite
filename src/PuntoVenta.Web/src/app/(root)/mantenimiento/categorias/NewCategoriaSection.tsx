"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewCategoriaForm } from "@pages/mantenimiento/categorias/NewCategoriaForm";

export default function NewCategoriaSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nueva categoría",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <NewCategoriaForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nueva Categoría
        </Button>
    );
}
