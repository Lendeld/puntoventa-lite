import { Flex, Group, Space, Stack, Text, Title } from "@mantine/core";
import type { Metadata } from "next";
import { LoginForm } from "@app/login/LoginForm";
import { obtenerRutaPostAutenticacionAction } from "@lib/actions/auth.actions";
import { redirect } from "next/navigation";
import { ROUTES } from "@lib/constants/routes.constants";

export const metadata: Metadata = {
    title: "Iniciar sesión",
};

export default async function LoginPage() {
    const ruta = await obtenerRutaPostAutenticacionAction();
    if (ruta && ruta !== ROUTES.LOGIN) {
        redirect(ruta);
    }

    return (
        <Flex className="min-h-dvh">
            {/* Panel izquierdo — branding */}
            <Flex
                direction="column"
                justify="space-between"
                className="hidden lg:flex w-1/2 p-10 overflow-hidden bg-linear-to-br from-[#f4f6f8] via-[#eef1f5] to-[#e4e9ef] dark:from-[#26282d] dark:via-[#2e3036] dark:to-[#32343a]"
            >
                {/* Logo */}
                <Group gap="sm" align="center">
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img
                        src="/icons/logo.svg"
                        alt="Punto Venta Lite"
                        width={40}
                        height={40}
                        className="size-10 shrink-0 shadow rounded-full"
                    />
                    <Stack gap={0}>
                        <Text
                            fw={700}
                            size="lg"
                            className="text-theme-text leading-tight"
                        >
                            Punto Venta Lite
                        </Text>
                        <Text size="xs" className="text-theme-text-muted">
                            Sistema POS
                        </Text>
                    </Stack>
                </Group>

                {/* Headline */}
                <Stack gap="xl">
                    <Title
                        order={2}
                        className="font-display text-5xl text-theme-text"
                    >
                        Tu negocio, <Space />
                        <Text
                            component="span"
                            className="font-display italic text-theme"
                            inherit
                        >
                            organizado
                        </Text>{" "}
                        en <Space />
                        un solo lugar.
                    </Title>
                    <Text
                        size="lg"
                        className="text-theme-text/80 max-w-sm leading-relaxed"
                    >
                        Vende, gestiona inventario, analiza tus ventas y cuida a
                        tus clientes, todo desde el mismo panel.
                    </Text>
                </Stack>

                {/* Footer */}
                <Text
                    size="xs"
                    className="text-theme-text-muted uppercase tracking-wide"
                >
                    © 2026 Punto Venta Lite
                </Text>
            </Flex>
            <LoginForm />
        </Flex>
    );
}
