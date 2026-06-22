import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import VendedoresPageSection from "@pages/mantenimiento/vendedores/VendedoresPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Vendedores",
};

export default async function VendedoresPage() {
    const [puedeCrear, puedeEditar] = await Promise.all([
        tienePermiso(PERMISOS.VENDEDORES_CREAR),
        tienePermiso(PERMISOS.VENDEDORES_EDITAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.VENDEDORES_VER}>
            <VendedoresPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
            />
        </PermisoServer>
    );
}
