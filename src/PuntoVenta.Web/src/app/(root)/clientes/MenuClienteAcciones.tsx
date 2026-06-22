import { Menu, UnstyledButton } from "@mantine/core";
import { IconDotsVertical } from "@tabler/icons-react";
import MenuEditarClienteAccion from "@pages/clientes/MenuEditarClienteAccion";

interface Props {
    id: string;
    puedeEditar: boolean;
}

export default function MenuClienteAcciones({ id, puedeEditar }: Props) {
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
                <MenuEditarClienteAccion id={id} />
            </Menu.Dropdown>
        </Menu>
    );
}
