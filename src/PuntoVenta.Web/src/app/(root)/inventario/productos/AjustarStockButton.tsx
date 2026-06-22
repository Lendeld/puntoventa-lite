"use client";

import AjustarStockModal from "@pages/inventario/productos/AjustarStockModal";
import { Button, Modal } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { IconAdjustments } from "@tabler/icons-react";

interface Props {
    id: string;
    nombre: string;
}

export default function AjustarStockButton({ id, nombre }: Props) {
    const [opened, { open, close }] = useDisclosure(false);

    return (
        <>
            <Button
                variant="light"
                leftSection={<IconAdjustments size={16} />}
                onClick={open}
            >
                Ajustar stock
            </Button>
            <Modal
                opened={opened}
                onClose={close}
                title="Ajuste de stock"
                centered
            >
                <AjustarStockModal
                    productoId={id}
                    productoNombre={nombre}
                    onClose={close}
                />
            </Modal>
        </>
    );
}
