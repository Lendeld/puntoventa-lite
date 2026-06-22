import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import EditCategoriaForm from "@pages/mantenimiento/categorias/EditCategoriaForm";

interface Props {
    id: string;
}

export default function MenuEditarCategoriaAccion({ id }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar categoría",
            centered: true,
            size: "lg",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditCategoriaForm id={id} />,
        });
    }

    return (
        <Menu.Item leftSection={<IconEdit size={16} />} onClick={handleOpenModal}>
            Editar
        </Menu.Item>
    );
}
