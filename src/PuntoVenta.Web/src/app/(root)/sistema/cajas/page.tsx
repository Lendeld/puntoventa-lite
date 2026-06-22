import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import CajasPageSection from "@pages/cajas/CajasPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Cajas",
};

export default async function CajasPage() {
    const [puedeCrear, puedeEditar, puedeToggle] = await Promise.all([
        tienePermiso(PERMISOS.CAJAS_CREAR),
        tienePermiso(PERMISOS.CAJAS_EDITAR),
        tienePermiso(PERMISOS.CAJAS_TOGGLE),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.CAJAS_VER}>
            <CajasPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
                puedeToggle={puedeToggle}
            />
        </PermisoServer>
    );
}
