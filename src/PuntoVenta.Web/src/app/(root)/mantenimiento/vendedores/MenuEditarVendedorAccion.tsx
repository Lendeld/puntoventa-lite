import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditVendedorForm from "@pages/mantenimiento/vendedores/EditVendedorForm";

interface Props {
    id: string;
}

export default function MenuEditarVendedorAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar vendedor",
            centered: true,
            size: "md",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditVendedorForm id={id} />,
        });
    }

    return (
        <Menu.Item leftSection={<IconEdit size={16} />} onClick={handleOpenModal}>
            Editar
        </Menu.Item>
    );
}
