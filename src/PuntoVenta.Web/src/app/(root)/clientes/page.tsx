import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import ClientesPageSection from "@pages/clientes/ClientesPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Clientes",
};

export default async function ClientesPage() {
    const [puedeCrear, puedeEditar] = await Promise.all([
        tienePermiso(PERMISOS.CLIENTES_CREAR),
        tienePermiso(PERMISOS.CLIENTES_EDITAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.CLIENTES_VER}>
            <ClientesPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
            />
        </PermisoServer>
    );
}
