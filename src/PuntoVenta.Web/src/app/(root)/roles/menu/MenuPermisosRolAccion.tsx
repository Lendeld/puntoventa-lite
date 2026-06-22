"use client";

import { Menu } from "@mantine/core";
import { ROUTES } from "@lib/constants/routes.constants";
import { IconShieldLock } from "@tabler/icons-react";
import Link from "next/link";

interface Props {
    id: string;
}

export default function MenuPermisosRolAccion({ id }: Props) {
    return (
        <Menu.Item
            component={Link}
            href={`${ROUTES.ROLES}/${id}/permisos`}
            leftSection={<IconShieldLock size={16} />}
        >
            Permisos
        </Menu.Item>
    );
}
