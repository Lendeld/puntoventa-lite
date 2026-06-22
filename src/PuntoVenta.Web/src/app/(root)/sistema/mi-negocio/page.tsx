import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { ROUTES } from "@/lib/constants/routes.constants";
import MiNegocioPageSection from "@pages/sistema/mi-negocio/MiNegocioPageSection";
import { Metadata } from "next";
import { redirect } from "next/navigation";

export const metadata: Metadata = {
    title: "Mi Negocio",
};

export default async function MiNegocioPage() {
    const [puedeVerNegocio, puedeEditarNegocio] = await Promise.all([
        tienePermiso(PERMISOS.NEGOCIO_VER),
        tienePermiso(PERMISOS.NEGOCIO_EDITAR),
    ]);

    if (!puedeVerNegocio) {
        redirect(ROUTES.PERMISO_DENEGADO);
    }

    return (
        <PermisoServer permiso={PERMISOS.NEGOCIO_VER}>
            <MiNegocioPageSection
                puedeEditarNegocio={puedeEditarNegocio}
            />
        </PermisoServer>
    );
}
