import { ColorSchemeScript, mantineHtmlProps } from "@mantine/core";
import type { Metadata } from "next";
import { GeistSans } from "geist/font/sans";
import { GeistMono } from "geist/font/mono";
import localFont from "next/font/local";
import "./globals.css";
import { Providers } from "@components/ui/Providers";
import { APP } from "@lib/constants/app.constants";
import { NuqsAdapter } from "nuqs/adapters/next/app";

const satoshi = localFont({
    src: "./fonts/satoshi-variable.woff2",
    weight: "300 900",
    variable: "--font-satoshi",
    display: "swap",
});

export const metadata: Metadata = {
    title: {
        template: `%s${APP.SEPARADOR_TITULO}${APP.NOMBRE}`,
        default: APP.NOMBRE,
    },
    description: APP.DESCRIPCION,
    icons: {
        icon: [{ url: "/icons/logo.svg", type: "image/svg+xml" }],
        shortcut: "/icons/logo.svg",
        apple: "/icons/logo.svg",
    },
};

export default function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <html
            lang="es"
            {...mantineHtmlProps}
            suppressHydrationWarning
            className={`${GeistSans.variable} ${GeistMono.variable} ${satoshi.variable}`}
        >
            <head>
                <ColorSchemeScript defaultColorScheme="auto" />
            </head>
            <body className="antialiased font-sans">
                <NuqsAdapter>
                    <Providers>{children}</Providers>
                </NuqsAdapter>
            </body>
        </html>
    );
}
