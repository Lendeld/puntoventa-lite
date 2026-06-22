import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { ROUTES } from "@/lib/constants/routes.constants";
import CatalogosPageSection from "@pages/sistema/catalogos/CatalogosPageSection";
import { Metadata } from "next";
import { redirect } from "next/navigation";
import { Suspense } from "react";

export const metadata: Metadata = {
    title: "Catálogos",
};

export default async function ConfiguracionPage() {
    const [
        puedeVerTipos,
        puedeToggleTipos,
        puedeVerCondicionesVenta,
        puedeToggleCondicionesVenta,
        puedeVerMediosPago,
        puedeToggleMediosPago,
        puedeVerCodigosImpuesto,
        puedeToggleCodigosImpuesto,
        puedeVerTarifasIva,
        puedeToggleTarifasIva,
    ] = await Promise.all([
        tienePermiso(PERMISOS.TIPOS_IDENTIFICACION_VER),
        tienePermiso(PERMISOS.TIPOS_IDENTIFICACION_TOGGLE),
        tienePermiso(PERMISOS.CONDICIONES_VENTA_VER),
        tienePermiso(PERMISOS.CONDICIONES_VENTA_TOGGLE),
        tienePermiso(PERMISOS.MEDIOS_PAGO_VER),
        tienePermiso(PERMISOS.MEDIOS_PAGO_TOGGLE),
        tienePermiso(PERMISOS.CODIGOS_IMPUESTO_VER),
        tienePermiso(PERMISOS.CODIGOS_IMPUESTO_TOGGLE),
        tienePermiso(PERMISOS.TARIFAS_IVA_VER),
        tienePermiso(PERMISOS.TARIFAS_IVA_TOGGLE),
    ]);

    if (
        !puedeVerTipos &&
        !puedeVerCondicionesVenta &&
        !puedeVerMediosPago &&
        !puedeVerCodigosImpuesto &&
        !puedeVerTarifasIva
    ) {
        redirect(ROUTES.PERMISO_DENEGADO);
    }

    return (
        <PermisoServer permiso={PERMISOS.CATALOGOS_VER}>
            <Suspense>
                <CatalogosPageSection
                    puedeVerTipos={puedeVerTipos}
                    puedeToggleTipos={puedeToggleTipos}
                    puedeVerCondicionesVenta={puedeVerCondicionesVenta}
                    puedeToggleCondicionesVenta={puedeToggleCondicionesVenta}
                    puedeVerMediosPago={puedeVerMediosPago}
                    puedeToggleMediosPago={puedeToggleMediosPago}
                    puedeVerCodigosImpuesto={puedeVerCodigosImpuesto}
                    puedeToggleCodigosImpuesto={puedeToggleCodigosImpuesto}
                    puedeVerTarifasIva={puedeVerTarifasIva}
                    puedeToggleTarifasIva={puedeToggleTarifasIva}
                />
            </Suspense>
        </PermisoServer>
    );
}
