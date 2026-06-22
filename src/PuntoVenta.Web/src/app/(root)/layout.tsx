import { redirect } from "next/navigation";
import { obtenerSesion } from "@lib/auth/sesion";
import { TokenValidator } from "@/components/auth/TokenValidator";
import { MasterLayout } from "@/components/ui/MasterLayout";
import { TerminosGate } from "@/components/terminos/TerminosGate";
import { asegurarAccessToken } from "@/lib/utils/apiClient";
import { obtenerEstadoTerminosService } from "@lib/services/terminos.service";
import { ROUTES } from "@/lib/constants/routes.constants";

export const dynamic = "force-dynamic";

export default async function RootLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const sesion = await obtenerSesion();

    if (!sesion?.accessToken && !sesion?.refreshToken) {
        redirect(ROUTES.API_LOGOUT);
    }

    const estado = await asegurarAccessToken();
    if (estado === "no-auth") {
        redirect(ROUTES.API_LOGOUT);
    }

    if (sesion.requiereCambioPassword) {
        redirect(ROUTES.CAMBIAR_PASSWORD);
    }

    // Aceptación de términos: bloquea el uso hasta aceptar la versión vigente.
    // Si el endpoint falla (data nula), no bloqueamos para no dejar afuera al usuario.
    const { data: estadoTerminos } = await obtenerEstadoTerminosService();
    if (estadoTerminos && !estadoTerminos.aceptado) {
        return <TerminosGate version={estadoTerminos.versionVigente} />;
    }

    return (
        <>
            <TokenValidator />
            <MasterLayout>{children}</MasterLayout>
        </>
    );
}
