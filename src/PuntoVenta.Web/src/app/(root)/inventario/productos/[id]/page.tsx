import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { obtenerProductoPorIdService } from "@lib/services/productos.service";
import DetalleProductoPageSection from "@pages/inventario/productos/[id]/DetalleProductoPageSection";
import type { Metadata } from "next";
import { notFound } from "next/navigation";

export const metadata: Metadata = {
    title: "Detalle producto",
};

interface Props {
    params: Promise<{ id: string }>;
}

export default async function DetalleProductoPage({ params }: Props) {
    const { id } = await params;

    const [productoResult, puedeEditar, puedeAjustarStock] = await Promise.all([
        obtenerProductoPorIdService(id),
        tienePermiso(PERMISOS.PRODUCTOS_EDITAR),
        tienePermiso(PERMISOS.PRODUCTOS_AJUSTAR_STOCK),
    ]);

    if (productoResult.errors || !productoResult.data) {
        notFound();
    }

    return (
        <PermisoServer permiso={PERMISOS.PRODUCTOS_VER}>
            <DetalleProductoPageSection
                producto={productoResult.data}
                permisos={{ editar: puedeEditar, ajustarStock: puedeAjustarStock }}
            />
        </PermisoServer>
    );
}
