"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewUsuarioForm } from "@pages/usuarios/new/NewUsuarioForm";

export default function NewUsuarioSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nuevo usuario",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                backgroundOpacity: 1,
                blur: 3,
            },
            children: <NewUsuarioForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nuevo usuario
        </Button>
    );
}
