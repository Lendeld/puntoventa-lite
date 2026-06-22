"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import {
    Alert,
    Button,
    Card,
    Checkbox,
    ScrollArea,
    Stack,
    Text,
    Title,
} from "@mantine/core";
import { aceptarTerminosAction } from "@lib/actions/terminos.actions";

const CLAUSULAS: { titulo: string; texto: string }[] = [
    {
        titulo: "1. Software «tal cual», sin garantías",
        texto: "Punto Venta Lite se ofrece de forma gratuita y «tal cual» (AS IS), sin garantías de ningún tipo, expresas o implícitas, incluyendo —sin limitarse a— garantías de funcionamiento continuo, exactitud, disponibilidad o adecuación para un fin particular. No se garantiza que el software esté libre de errores ni que funcione sin interrupciones.",
    },
    {
        titulo: "2. Limitación de responsabilidad",
        texto: "En la máxima medida permitida por la ley, el autor no será responsable por ningún daño directo, indirecto, incidental o consecuente derivado del uso o de la imposibilidad de uso del software, incluyendo —sin limitarse a— pérdida de datos, pérdida de ingresos, lucro cesante, errores de cálculo o de facturación, o interrupciones del negocio.",
    },
    {
        titulo: "3. Responsabilidad del usuario",
        texto: "Sos responsable de verificar que tus facturas, impuestos y comprobantes cumplan con la normativa vigente, de mantener respaldos de tu información, de usar el software de forma correcta y para fines legales, y de la exactitud de los datos que ingresás.",
    },
    {
        titulo: "4. No reemplaza asesoría profesional",
        texto: "El software es una herramienta de apoyo y no sustituye la asesoría de un contador, abogado o profesional en materia fiscal o legal. Ante cualquier duda sobre tus obligaciones, consultá con un profesional.",
    },
    {
        titulo: "5. Uso bajo tu propio riesgo",
        texto: "El uso del software es bajo tu entera responsabilidad y riesgo. Vos asumís cualquier consecuencia derivada de su uso.",
    },
    {
        titulo: "6. Cumplimiento legal y fiscal",
        texto: "Punto Venta Lite está pensado para la forma de facturar de Costa Rica, pero no garantiza el cumplimiento de ninguna obligación legal o tributaria. Es tu responsabilidad asegurarte de cumplir con la legislación aplicable a tu negocio.",
    },
    {
        titulo: "7. Tus datos",
        texto: "El software funciona de forma local (offline): tus datos se guardan en tu propia computadora. No se recolecta ni se almacena tu información en servidores externos. El respaldo y la seguridad de tu equipo son tu responsabilidad.",
    },
];

export function TerminosGate({ version }: { version: string }) {
    const router = useRouter();
    const [acepta, setAcepta] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [pending, startTransition] = useTransition();

    const continuar = () => {
        setError(null);
        startTransition(async () => {
            const res = await aceptarTerminosAction(version);
            if (res.status >= 400) {
                setError("No se pudo registrar la aceptación. Intentá de nuevo.");
                return;
            }
            router.refresh();
        });
    };

    return (
        <div className="flex min-h-dvh items-center justify-center p-4">
            <Card padding="xl" className="w-full max-w-2xl">
                <Stack gap="md">
                    <div>
                        <Text className="text-2xs uppercase tracking-wide text-theme-text-dim">
                            Antes de empezar
                        </Text>
                        <Title order={2} className="font-display text-2xl text-theme-text">
                            Términos y condiciones de uso
                        </Title>
                        <Text size="sm" className="mt-1 text-theme-text-muted">
                            Leé y aceptá los términos para usar Punto Venta Lite.
                        </Text>
                    </div>

                    <ScrollArea
                        h={320}
                        type="auto"
                        className="rounded-md border border-theme-border-soft bg-theme-surface"
                    >
                        <Stack gap="md" className="p-4">
                            <Text size="sm" className="text-theme-text-muted">
                                Al usar Punto Venta Lite aceptás estos términos. Si no estás
                                de acuerdo, no lo uses.
                            </Text>
                            {CLAUSULAS.map((c) => (
                                <div key={c.titulo}>
                                    <Text size="sm" fw={600} className="text-theme-text">
                                        {c.titulo}
                                    </Text>
                                    <Text size="sm" className="mt-1 text-theme-text-muted">
                                        {c.texto}
                                    </Text>
                                </div>
                            ))}
                        </Stack>
                    </ScrollArea>

                    {error && (
                        <Alert color="red" variant="light">
                            {error}
                        </Alert>
                    )}

                    <Checkbox
                        checked={acepta}
                        onChange={(e) => setAcepta(e.currentTarget.checked)}
                        label="He leído y acepto los términos y condiciones"
                    />

                    <Button
                        onClick={continuar}
                        disabled={!acepta}
                        loading={pending}
                        fullWidth
                    >
                        Aceptar y continuar
                    </Button>
                </Stack>
            </Card>
        </div>
    );
}
