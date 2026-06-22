import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditClienteForm from "@pages/clientes/EditClienteForm";

interface Props {
    id: string;
}

export default function MenuEditarClienteAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar cliente",
            centered: true,
            size: "xl",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditClienteForm id={id} />,
        });
    }

    return (
        <Menu.Item leftSection={<IconEdit size={16} />} onClick={handleOpenModal}>
            Editar
        </Menu.Item>
    );
}
