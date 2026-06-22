import { Menu, UnstyledButton } from "@mantine/core";
import MenuEditarUsuarioAccion from "@pages/usuarios/menu/MenuEditarAccion";
import { IconDotsVertical } from "@tabler/icons-react";

interface Props {
    id: string;
}

export default function MenuUsuarioAcciones({ id }: Props) {
    return (
        <Menu shadow="md" width={180} position="bottom-end">
            <Menu.Target>
                <UnstyledButton className="inline-flex items-center justify-center rounded-md p-1.5 hover:bg-theme">
                    <IconDotsVertical size={18} />
                </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
                <MenuEditarUsuarioAccion id={id} />
            </Menu.Dropdown>
        </Menu>
    );
}
