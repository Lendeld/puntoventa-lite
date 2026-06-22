import { Menu, UnstyledButton } from "@mantine/core";
import { IconDotsVertical } from "@tabler/icons-react";
import MenuEditarCategoriaAccion from "@pages/mantenimiento/categorias/MenuEditarCategoriaAccion";

interface Props {
    id: string;
    puedeEditar: boolean;
}

export default function MenuCategoriaAcciones({ id, puedeEditar }: Props) {
    if (!puedeEditar) {
        return null;
    }

    return (
        <Menu shadow="md" width={180} position="bottom-end">
            <Menu.Target>
                <UnstyledButton className="inline-flex items-center justify-center rounded-md p-1.5 hover:bg-theme">
                    <IconDotsVertical size={18} />
                </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
                <MenuEditarCategoriaAccion id={id} />
            </Menu.Dropdown>
        </Menu>
    );
}
