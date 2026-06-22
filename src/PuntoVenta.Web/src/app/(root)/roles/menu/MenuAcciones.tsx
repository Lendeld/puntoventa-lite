import { Menu, UnstyledButton } from "@mantine/core";
import MenuEditarRolAccion from "@pages/roles/menu/MenuEditarAccion";
import MenuPermisosRolAccion from "@pages/roles/menu/MenuPermisosRolAccion";
import { IconDotsVertical } from "@tabler/icons-react";

interface Props {
    id: string;
    isPrincipal: boolean;
}

export default function MenuRolAcciones({ id, isPrincipal }: Props) {
    return (
        <Menu shadow="md" width={180} position="bottom-end">
            <Menu.Target>
                <UnstyledButton className="inline-flex items-center justify-center rounded-md p-1.5 hover:bg-theme">
                    <IconDotsVertical size={18} />
                </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
                {!isPrincipal && <MenuPermisosRolAccion id={id} />}
                <MenuEditarRolAccion id={id} />
            </Menu.Dropdown>
        </Menu>
    );
}
