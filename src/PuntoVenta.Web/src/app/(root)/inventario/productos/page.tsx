import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import ProductosPageSection from "@pages/inventario/productos/ProductosPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Productos",
};

export default async function ProductosPage() {
    const [puedeCrear, puedeEditar, puedeAjustarStock] = await Promise.all([
        tienePermiso(PERMISOS.PRODUCTOS_CREAR),
        tienePermiso(PERMISOS.PRODUCTOS_EDITAR),
        tienePermiso(PERMISOS.PRODUCTOS_AJUSTAR_STOCK),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.PRODUCTOS_VER}>
            <ProductosPageSection
                puedeCrear={puedeCrear}
                puedeEditar={puedeEditar}
                puedeAjustarStock={puedeAjustarStock}
            />
        </PermisoServer>
    );
}
