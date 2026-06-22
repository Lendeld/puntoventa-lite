import type { NextConfig } from "next";

const backendApiUrl = process.env.BASE_URL_API ?? "http://localhost:5159";

const DEV_WS = "ws://localhost:* ws://127.0.0.1:*";

// Cuando se buildea para empaquetar dentro de Electron (PuntoVenta.Desktop)
// se activa `output: 'standalone'` para generar un `.next/standalone/server.js`
// self-contained que Electron arranca como child Node process.
// En deploy normal la var no está y el output queda default.
const isDesktopBuild = process.env.DESKTOP_BUILD === "true";

// Electron arranca `next dev` apuntando a un distDir separado para que
// pueda coexistir con el `pnpm dev` normal (mismo proyecto, otra .next).
// Sin esto, Next bloquea con "Another next dev server is already running"
// porque ambos comparten el lock en .next/.
const distDirOverride = process.env.NEXT_DIST_DIR?.trim();

const nextConfig: NextConfig = {
    ...(isDesktopBuild ? { output: "standalone" as const } : {}),
    ...(distDirOverride ? { distDir: distDirOverride } : {}),
    reactCompiler: true,
    // API proxy to backend
    async rewrites() {
        return [
            {
                source: "/api/:path((?!auth/).*)",
                destination: `${backendApiUrl}/api/:path*`,
            },
        ];
    },
    async headers() {
        return [
            {
                source: "/(.*)",
                headers: [
                    { key: "X-Frame-Options", value: "DENY" },
                    { key: "X-Content-Type-Options", value: "nosniff" },
                    { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
                    { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
                    {
                        key: "Content-Security-Policy",
                        value: [
                            "default-src 'self'",
                            "script-src 'self' 'unsafe-inline' 'unsafe-eval'",
                            "style-src 'self' 'unsafe-inline'",
                            "img-src 'self' data: blob:",
                            "font-src 'self' data:",
                            [
                                "connect-src 'self'",
                                process.env.NODE_ENV === "development" && DEV_WS,
                            ]
                                .filter(Boolean)
                                .join(" "),
                            "frame-ancestors 'none'",
                        ].join("; "),
                    },
                ],
            },
        ];
    },
};

export default nextConfig;
