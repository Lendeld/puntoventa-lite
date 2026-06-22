import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReporteMovimientosDineroPageSection } from "@pages/reportes-ventas/movimientos-dinero/ReporteMovimientosDineroPageSection";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Movimientos de dinero",
};

export default function ReporteMovimientosDineroPage() {
    return (
        <PermisoServer permiso={PERMISOS.REPORTES_VER}>
            <ReporteMovimientosDineroPageSection />
        </PermisoServer>
    );
}
