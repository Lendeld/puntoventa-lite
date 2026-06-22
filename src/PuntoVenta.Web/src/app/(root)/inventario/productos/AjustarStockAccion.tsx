"use client";

import AjustarStockModal from "@pages/inventario/productos/AjustarStockModal";
import { Menu } from "@mantine/core";
import { modals } from "@mantine/modals";
import { IconAdjustments } from "@tabler/icons-react";

interface Props {
    id: string;
    nombre: string;
}

export default function AjustarStockAccion({ id, nombre }: Props) {
    // El modal se abre vía el modals manager (montado en el root por
    // ModalsProvider) y NO como hijo del Menu.Dropdown: al hacer clic el menú
    // cierra y desmonta su dropdown, lo que mataría un <Modal> anidado.
    function abrir() {
        const modalId = modals.open({
            title: "Ajuste de stock",
            centered: true,
            children: (
                <AjustarStockModal
                    productoId={id}
                    productoNombre={nombre}
                    onClose={() => modals.close(modalId)}
                />
            ),
        });
    }

    return (
        <Menu.Item leftSection={<IconAdjustments size={16} />} onClick={abrir}>
            Ajustar stock
        </Menu.Item>
    );
}
