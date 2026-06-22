import PermisoServer from "@/components/auth/PermisoServer";
import { tienePermiso } from "@/lib/auth/permisos";
import { PERMISOS } from "@/lib/constants/permisos.constants";
import UsuariosPageSection from "@pages/usuarios/UsuariosPageSection";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Usuarios",
};

export default async function UsuariosPage() {
    const [puedeCrearUsuarios, puedeEditarUsuarios] = await Promise.all([
        tienePermiso(PERMISOS.USUARIOS_CREAR),
        tienePermiso(PERMISOS.USUARIOS_EDITAR),
    ]);

    return (
        <PermisoServer permiso={PERMISOS.USUARIOS_VER}>
            <UsuariosPageSection
                puedeCrearUsuarios={puedeCrearUsuarios}
                puedeEditarUsuarios={puedeEditarUsuarios}
            />
        </PermisoServer>
    );
}
