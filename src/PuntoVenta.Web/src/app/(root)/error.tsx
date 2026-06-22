"use client";

import { ROUTES } from "@lib/constants/routes.constants";
import { Alert, Button, Group, Stack } from "@mantine/core";
import { IconAlertTriangle } from "@tabler/icons-react";
import { useEffect } from "react";

export default function AppError({
    error,
    reset,
}: {
    error: Error & { digest?: string };
    reset: () => void;
}) {
    useEffect(() => {
        console.error(error);
    }, [error]);

    return (
        <Stack justify="center" align="center" className="h-page" gap="lg">
            <Alert
                icon={<IconAlertTriangle size={18} />}
                color="red"
                title="Algo salió mal"
                maw={520}
            >
                Ocurrió un error inesperado. Podés reintentar o volver al inicio.
            </Alert>
            <Group>
                <Button variant="light" onClick={reset}>
                    Reintentar
                </Button>
                <Button component="a" href={ROUTES.HOME}>
                    Volver al inicio
                </Button>
            </Group>
        </Stack>
    );
}
