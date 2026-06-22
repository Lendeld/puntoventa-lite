"use client";

import { Menu } from "@mantine/core";
import { IconEdit } from "@tabler/icons-react";
import Link from "next/link";

interface Props {
    id: string;
}

export default function MenuEditarProductoAccion({ id }: Props) {
    return (
        <Menu.Item
            component={Link}
            href={`/inventario/productos/${id}/editar`}
            leftSection={<IconEdit size={16} />}
        >
            Editar
        </Menu.Item>
    );
}
