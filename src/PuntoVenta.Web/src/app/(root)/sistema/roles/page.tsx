import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import RolesPageSection from "@pages/roles/RolesPageSection";

export const metadata = {
    title: "Roles",
};

export default function RolesPage() {
    return (
        <PermisoServer permiso={PERMISOS.ROLES_VER}>
            <RolesPageSection />
        </PermisoServer>
    );
}
