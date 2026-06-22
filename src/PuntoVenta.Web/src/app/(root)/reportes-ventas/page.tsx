import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReportesVentasHubSection } from "@pages/reportes-ventas/ReportesVentasHubSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Reportes Ventas",
};

export default function ReportesVentasPage() {
    return (
        <PermisoServer permiso={PERMISOS.REPORTES_VER}>
            <ReportesVentasHubSection />
        </PermisoServer>
    );
}
