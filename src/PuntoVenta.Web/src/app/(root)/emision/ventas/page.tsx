import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import VentasPageSection from "@pages/emision/ventas/VentasPageSection";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Ventas",
};

export default async function EmisionVentasPage() {
    const puedeVerVentas = await tienePermiso(PERMISOS.FACTURACION_VER);

    return (
        <PermisoServer permiso={PERMISOS.FACTURACION_VER}>
            {puedeVerVentas ? <VentasPageSection /> : null}
        </PermisoServer>
    );
}
