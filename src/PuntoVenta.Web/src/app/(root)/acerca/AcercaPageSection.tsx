import { Alert, Card, Group, Stack, Text, Title } from "@mantine/core";
import type { AcercaDto } from "@lib/types/acerca.types";

interface AcercaPageSectionProps {
    data: AcercaDto | null;
}

export function AcercaPageSection({ data }: AcercaPageSectionProps) {
    return (
        <Stack gap="xl" className="max-w-4xl mx-auto py-6">
            <Stack gap="xs">
                <Text
                    component="span"
                    className="text-2xs uppercase tracking-wide text-theme-text-dim"
                >
                    Acerca de
                </Text>
                <Title className="font-display text-4xl text-theme-text">
                    Punto Venta Lite
                </Title>
                <Text size="sm" className="text-theme-text-muted max-w-xl">
                    Información del sistema y del ambiente en que está corriendo.
                </Text>
            </Stack>

            {!data && (
                <Alert color="red" title="No se pudo cargar la información del sistema">
                    Intenta recargar la página. Si el problema persiste, contacta a soporte.
                </Alert>
            )}

            {data && (
                <Card padding="lg">
                    <Stack gap="md">
                        <DatoFila etiqueta="Versión" valor={data.backendVersion} />
                        <DatoFila etiqueta="Ambiente" valor={data.ambiente} />
                    </Stack>
                </Card>
            )}

            <Text size="xs" className="text-theme-text-dim">
                © 2026 Punto Venta Lite
            </Text>
        </Stack>
    );
}

interface DatoFilaProps {
    etiqueta: string;
    valor: string;
}

function DatoFila({ etiqueta, valor }: DatoFilaProps) {
    return (
        <Group justify="space-between" align="center" gap="md">
            <Text size="sm" className="text-theme-text-muted">
                {etiqueta}
            </Text>
            <Text className="font-display text-theme-text tabular-nums" size="sm">
                {valor}
            </Text>
        </Group>
    );
}
