import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditUsuarioForm from "@pages/usuarios/edit/EditUsuarioForm";

interface Props {
    id: string;
}

export default function MenuEditarUsuarioAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar usuario",
            centered: true,
            size: "md",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditUsuarioForm id={id} />,
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
