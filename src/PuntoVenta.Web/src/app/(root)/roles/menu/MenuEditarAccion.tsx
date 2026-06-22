import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditRolForm from "@pages/roles/edit/EditRolForm";

interface Props {
    id: string;
}

export default function MenuEditarRolAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar rol",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditRolForm id={id} />,
        });
    }

    return (
        <Menu.Item
            leftSection={<IconEdit size={16} />}
            onClick={handleOpenModal}
        >
            Editar
        </Menu.Item>
    );
}
