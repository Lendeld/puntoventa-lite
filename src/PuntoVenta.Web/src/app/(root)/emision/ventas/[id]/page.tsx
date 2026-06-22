import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { obtenerDocumentoVentaPorIdService } from "@lib/services/ventas.service";
import DetalleVentaPageSection from "@pages/emision/ventas/[id]/DetalleVentaPageSection";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export const metadata: Metadata = {
    title: "Detalle venta",
};

interface Props {
    params: Promise<{
        id: string;
    }>;
}

export default async function DetalleVentaPage({ params }: Props) {
    const [puedeVerVentas, { id }] = await Promise.all([
        tienePermiso(PERMISOS.FACTURACION_VER),
        params,
    ]);

    if (!puedeVerVentas) {
        return (
            <PermisoServer permiso={PERMISOS.FACTURACION_VER}>
                {null}
            </PermisoServer>
        );
    }

    const [
        documentoResult,
        puedeAbonar,
        puedeExtender,
        puedeConvertir,
        puedeCancelar,
        puedeEmitirNC,
        puedeEmitirND,
        puedeAbonarCredito,
        puedeAnularAbono,
    ] = await Promise.all([
        obtenerDocumentoVentaPorIdService(id),
        tienePermiso(PERMISOS.VENTAS_APARTADOS_ABONAR),
        tienePermiso(PERMISOS.VENTAS_APARTADOS_EXTENDER),
        tienePermiso(PERMISOS.VENTAS_APARTADOS_CONVERTIR),
        tienePermiso(PERMISOS.VENTAS_APARTADOS_CANCELAR),
        tienePermiso(PERMISOS.VENTAS_NOTAS_CREDITO_CREAR),
        tienePermiso(PERMISOS.VENTAS_NOTAS_DEBITO_CREAR),
        tienePermiso(PERMISOS.VENTAS_FACTURAS_ABONAR),
        tienePermiso(PERMISOS.VENTAS_FACTURAS_ABONO_ANULAR),
    ]);

    if (documentoResult.errors || !documentoResult.data) {
        notFound();
    }

    return (
        <PermisoServer permiso={PERMISOS.FACTURACION_VER}>
            <Suspense>
                <DetalleVentaPageSection
                    documento={documentoResult.data}
                    permisos={{
                        abonar: puedeAbonar,
                        extender: puedeExtender,
                        convertir: puedeConvertir,
                        cancelar: puedeCancelar,
                        emitirNotaCredito: puedeEmitirNC,
                        emitirNotaDebito: puedeEmitirND,
                        abonarCredito: puedeAbonarCredito,
                        anularAbono: puedeAnularAbono,
                    }}
                />
            </Suspense>
        </PermisoServer>
    );
}
