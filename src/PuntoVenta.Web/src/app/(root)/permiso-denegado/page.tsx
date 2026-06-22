import { ROUTES } from "@lib/constants/routes.constants";
import { Alert, Button, Group, Stack } from "@mantine/core";
import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Acceso restringido",
};

export default function PermisoDenegadoPage() {
    return (
        <Stack justify="center" align="center" className="h-page" gap="lg">
            <Alert color="red" title="Acceso restringido" maw={520}>
                Tu usuario no cuenta con permiso para entrar a este recurso.
            </Alert>
            <Group>
                <Button component="a" href={ROUTES.HOME}>
                    Volver al inicio
                </Button>
            </Group>
        </Stack>
    );
}
