import { obtenerRolPorIdService } from "@lib/services/roles.service";
import type { RolDto } from "@lib/types/roles.types";
import PermisoServer from "@components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ROUTES } from "@lib/constants/routes.constants";
import PermisosRolPageSection from "@pages/roles/[id]/permisos/PermisosRolPageSection";
import { redirect } from "next/navigation";

export const metadata = {
    title: "Permisos del Rol",
};

interface Props {
    params: Promise<{
        id: string;
    }>;
}

export default async function RolPermisosPage({ params }: Props) {
    const { id } = await params;
    const response = await obtenerRolPorIdService(id);

    if (response.errors || !response.data) {
        redirect(ROUTES.ROLES);
    }

    const rol: RolDto = response.data;

    if (rol.isPrincipal) {
        redirect(ROUTES.ROLES);
    }

    return (
        <PermisoServer permiso={PERMISOS.ROLES_PERMISOS_ADMINISTRAR}>
            <PermisosRolPageSection id={id} initialRol={rol} />
        </PermisoServer>
    );
}
