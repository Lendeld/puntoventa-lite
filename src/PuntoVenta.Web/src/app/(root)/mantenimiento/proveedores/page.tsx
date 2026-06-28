import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import ProveedoresPageSection from "@pages/mantenimiento/proveedores/ProveedoresPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Proveedores",
};

export default async function ProveedoresPage() {
    const [puedeCrear, puedeEditar] = await Promise.all([
        tienePermiso(PERMISOS.PROVEEDORES_CREAR),
        tienePermiso(PERMISOS.PROVEEDORES_EDITAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.PROVEEDORES_VER}>
            <ProveedoresPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
            />
        </PermisoServer>
    );
}
