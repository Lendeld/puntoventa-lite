import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconEdit } from "@tabler/icons-react";
import type { CajaListadoItemDto } from "@lib/types/cajas.types";
import EditCajaForm from "@pages/cajas/EditCajaForm";

interface Props {
    caja: CajaListadoItemDto;
}

export default function MenuEditarCajaAccion({ caja }: Props) {
    function handleOpenModal() {
        modals.open({
            title: "Editar caja",
            centered: true,
            size: "md",
            closeOnClickOutside: false,
            closeOnEscape: false,
            overlayProps: {
                opacity: 1,
                blur: 3,
            },
            children: <EditCajaForm caja={caja} />,
        });
    }

    return (
        <Menu.Item leftSection={<IconEdit size={16} />} onClick={handleOpenModal}>
            Editar
        </Menu.Item>
    );
}
