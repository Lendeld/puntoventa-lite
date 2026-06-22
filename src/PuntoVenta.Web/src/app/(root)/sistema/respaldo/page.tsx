import PermisoServer from "@/components/auth/PermisoServer";
import { PERMISOS } from "@lib/constants/permisos.constants";
import RespaldoPageSection from "@pages/sistema/respaldo/RespaldoPageSection";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Respaldo de base de datos",
};

export default function RespaldoPage() {
    return (
        <PermisoServer permiso={PERMISOS.BACKUP_ADMINISTRAR}>
            <RespaldoPageSection />
        </PermisoServer>
    );
}
