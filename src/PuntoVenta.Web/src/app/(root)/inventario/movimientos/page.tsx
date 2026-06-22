import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import MovimientosStockPageSection from "@pages/inventario/movimientos/MovimientosStockPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Movimientos",
};

export default async function MovimientosStockPage() {
    return (
        <PermisoServer permiso={PERMISOS.PRODUCTOS_MOVIMIENTOS_VER}>
            <MovimientosStockPageSection />
        </PermisoServer>
    );
}
