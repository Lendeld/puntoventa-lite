import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import CuentasPorCobrarSection from "./CuentasPorCobrarSection";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Cuentas por Cobrar",
};

export default async function CobrosCreditoPage() {
    const [puedeVer, puedeAbonar] = await Promise.all([
        tienePermiso(PERMISOS.VENTAS_CREDITO_VER),
        tienePermiso(PERMISOS.VENTAS_FACTURAS_ABONAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.VENTAS_CREDITO_VER}>
            {puedeVer ? <CuentasPorCobrarSection puedeAbonar={puedeAbonar} /> : null}
        </PermisoServer>
    );
}
