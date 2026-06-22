import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import { BackButton } from "@components/ui/BackButton";
import EditProductoForm from "@pages/inventario/productos/EditProductoForm";
import { Card, Stack } from "@mantine/core";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Editar producto",
};

interface Props {
    params: Promise<{ id: string }>;
}

export default async function EditarProductoPage({ params }: Props) {
    const [{ id }, puedeGestionarNoAplicaExistencias] = await Promise.all([
        params,
        tienePermiso(PERMISOS.PRODUCTOS_NO_APLICA_EXISTENCIAS),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.PRODUCTOS_EDITAR}>
            <Stack gap="lg">
                <BackButton
                    href={`/inventario/productos/${id}`}
                    label="Volver al detalle"
                />

                <Card
                    radius="lg"
                    p="lg"
                   
                >
                    <EditProductoForm
                        id={id}
                        puedeGestionarNoAplicaExistencias={
                            puedeGestionarNoAplicaExistencias
                        }
                    />
                </Card>
            </Stack>
        </PermisoServer>
    );
}
