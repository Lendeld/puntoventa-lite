import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReporteVentasRangoPageSection } from "@pages/reportes-ventas/ventas-rango/ReporteVentasRangoPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Reporte de ventas por rango",
};

export default function ReporteVentasRangoPage() {
    return (
        <PermisoServer permiso={PERMISOS.REPORTES_VENTAS_RANGO_VER}>
            <ReporteVentasRangoPageSection />
        </PermisoServer>
    );
}
