import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReporteExistenciasPageSection } from "@pages/reportes-inventario/existencias/ReporteExistenciasPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Existencias",
};

export default function ReporteExistenciasPage() {
    return (
        <PermisoServer permiso={PERMISOS.REPORTES_INVENTARIO_VER}>
            <ReporteExistenciasPageSection />
        </PermisoServer>
    );
}
