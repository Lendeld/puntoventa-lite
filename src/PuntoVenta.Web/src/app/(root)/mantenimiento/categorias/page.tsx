import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import CategoriasPageSection from "@pages/mantenimiento/categorias/CategoriasPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Categorías",
};

export default async function CategoriasPage() {
    const [puedeCrear, puedeEditar] = await Promise.all([
        tienePermiso(PERMISOS.CATEGORIAS_CREAR),
        tienePermiso(PERMISOS.CATEGORIAS_EDITAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.CATEGORIAS_VER}>
            <CategoriasPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
            />
        </PermisoServer>
    );
}
