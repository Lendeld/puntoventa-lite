import { Menu, UnstyledButton } from "@mantine/core";
import { IconDotsVertical } from "@tabler/icons-react";
import type { CajaListadoItemDto } from "@lib/types/cajas.types";
import MenuEditarCajaAccion from "@pages/cajas/MenuEditarCajaAccion";
import MenuToggleCajaAccion from "@pages/cajas/MenuToggleCajaAccion";

interface Props {
    caja: CajaListadoItemDto;
    puedeEditar: boolean;
    puedeToggle: boolean;
}

export default function MenuCajaAcciones({ caja, puedeEditar, puedeToggle }: Props) {
    if (!puedeEditar && !puedeToggle) {
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
                {puedeEditar && <MenuEditarCajaAccion caja={caja} />}
                {puedeToggle && (
                    <MenuToggleCajaAccion id={caja.id} activo={caja.activo} />
                )}
            </Menu.Dropdown>
        </Menu>
    );
}
