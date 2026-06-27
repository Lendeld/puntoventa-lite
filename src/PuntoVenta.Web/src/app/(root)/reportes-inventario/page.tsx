import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import { ReportesInventarioHubSection } from "@pages/reportes-inventario/ReportesInventarioHubSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Reportes Inventario",
};

export default function ReportesInventarioPage() {
    return (
        <PermisoServer permiso={PERMISOS.REPORTES_VER}>
            <ReportesInventarioHubSection />
        </PermisoServer>
    );
}
