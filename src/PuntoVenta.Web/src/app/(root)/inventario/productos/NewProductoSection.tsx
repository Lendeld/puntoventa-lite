"use client";

import { Button } from "@mantine/core";
import { IconPlus } from "@tabler/icons-react";
import Link from "next/link";

export default function NewProductoSection() {
    return (
        <Button
            component={Link}
            href="/inventario/productos/nuevo"
            leftSection={<IconPlus size={16} />}
        >
            Nuevo Producto
        </Button>
    );
}
