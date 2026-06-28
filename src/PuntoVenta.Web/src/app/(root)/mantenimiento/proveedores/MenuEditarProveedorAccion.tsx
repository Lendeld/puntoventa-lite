import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditProveedorForm from "@pages/mantenimiento/proveedores/EditProveedorForm";

interface Props {
    id: string;
}

export default function MenuEditarProveedorAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar proveedor",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditProveedorForm id={id} />,
        });
    }

    return (
        <Menu.Item leftSection={<IconEdit size={16} />} onClick={handleOpenModal}>
            Editar
        </Menu.Item>
    );
}
