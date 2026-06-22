import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { BackButton } from "@components/ui/BackButton";
import { NewProductoForm } from "@pages/inventario/productos/NewProductoForm";
import { Card, Stack } from "@mantine/core";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Nuevo producto",
};

export default async function NuevoProductoPage() {
    const puedeGestionarNoAplicaExistencias = await tienePermiso(
        PERMISOS.PRODUCTOS_NO_APLICA_EXISTENCIAS,
    );

    return (
        <PermisoServer permiso={PERMISOS.PRODUCTOS_CREAR}>
            <Stack gap="lg">
                <BackButton
                    href="/inventario/productos"
                    label="Volver a productos"
                />

                <Card
                    radius="lg"
                    p="lg"
                   
                >
                    <NewProductoForm
                        puedeGestionarNoAplicaExistencias={
                            puedeGestionarNoAplicaExistencias
                        }
                    />
                </Card>
            </Stack>
        </PermisoServer>
    );
}
