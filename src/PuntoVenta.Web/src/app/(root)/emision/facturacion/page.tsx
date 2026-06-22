import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import FacturacionPageSection from "@pages/emision/facturacion/FacturacionPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Facturación",
};

export default async function FacturacionPage() {
    const puedeUsarFacturacion = await tienePermiso(PERMISOS.FACTURACION_VER);

    return (
        <PermisoServer permiso={PERMISOS.FACTURACION_VER}>
            <FacturacionPageSection puedeFacturar={puedeUsarFacturacion} />
        </PermisoServer>
    );
}
