"use server";
import { redirect } from "next/navigation";
import { tienePermiso } from "@lib/auth/permisos";
import { ROUTES } from "@lib/constants/routes.constants";

interface Props {
    permiso: string;
    children: React.ReactNode;
    redirectTo?: string;
}

export default async function PermisoServer({
    permiso,
    children,
    redirectTo = ROUTES.PERMISO_DENEGADO,
}: Props) {
    const allowed = await tienePermiso(permiso);

    if (!allowed) return redirect(redirectTo);

    return children;
}
