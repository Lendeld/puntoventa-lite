"use client";

import { Button } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconPlus } from "@tabler/icons-react";
import { NewRolForm } from "@pages/roles/new/NewRolForm";

export default function NewRolSection() {
    function handleOpenModal() {
        modals.open({
            title: "Nuevo rol",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <NewRolForm />,
        });
    }

    return (
        <Button leftSection={<IconPlus size={16} />} onClick={handleOpenModal}>
            Nuevo Rol
        </Button>
    );
}
