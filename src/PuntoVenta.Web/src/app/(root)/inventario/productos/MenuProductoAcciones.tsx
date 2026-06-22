"use client";

import AjustarStockAccion from "@pages/inventario/productos/AjustarStockAccion";
import MenuEditarProductoAccion from "@pages/inventario/productos/MenuEditarProductoAccion";
import { Menu, UnstyledButton } from "@mantine/core";
import { IconDotsVertical, IconEye } from "@tabler/icons-react";
import Link from "next/link";

interface Props {
    id: string;
    nombre: string;
    puedeEditar: boolean;
    puedeAjustarStock: boolean;
}

export default function MenuProductoAcciones({
    id,
    nombre,
    puedeEditar,
    puedeAjustarStock,
}: Props) {
    return (
        <Menu shadow="md" width={190} position="bottom-end">
            <Menu.Target>
                <UnstyledButton className="inline-flex items-center justify-center rounded-md p-1.5 hover:bg-theme">
                    <IconDotsVertical size={18} />
                </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
                <Menu.Item
                    component={Link}
                    href={`/inventario/productos/${id}`}
                    leftSection={<IconEye size={16} />}
                >
                    Ver detalle
                </Menu.Item>
                {puedeEditar && <MenuEditarProductoAccion id={id} />}
                {puedeAjustarStock && <AjustarStockAccion id={id} nombre={nombre} />}
            </Menu.Dropdown>
        </Menu>
    );
}
