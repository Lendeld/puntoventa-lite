import { redirect } from "next/navigation";
import { obtenerSesion } from "@lib/auth/sesion";
import { ROUTES } from "@lib/constants/routes.constants";
import { asegurarAccessToken } from "@lib/utils/apiClient";
import { Box, Center, Container, Paper } from "@mantine/core";
import { CambiarPasswordObligatorioForm } from "@app/cambiar-password/CambiarPasswordObligatorioForm";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Cambiar contraseña",
};

export const dynamic = "force-dynamic";

export default async function CambiarPasswordPage() {
    const sesion = await obtenerSesion();

    if (!sesion.accessToken && !sesion.refreshToken) {
        redirect(ROUTES.LOGIN);
    }

    const estado = await asegurarAccessToken();
    if (estado === "no-auth") {
        redirect(ROUTES.API_LOGOUT);
    }

    if (!sesion.requiereCambioPassword) {
        redirect(ROUTES.HOME);
    }

    return (
        <Box className="min-h-dvh bg-theme-canvas py-12">
            <Container size={520}>
                <Center>
                    <Paper withBorder radius="md" p="xl" w="100%">
                        <CambiarPasswordObligatorioForm />
                    </Paper>
                </Center>
            </Container>
        </Box>
    );
}
