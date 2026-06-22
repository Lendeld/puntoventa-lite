"use client";

// global-error reemplaza el layout raíz, así que renderiza su propio <html>/<body>
// y no depende de Providers (que podrían ser justo lo que falló). Estilos vía
// tokens de tema de globals.css; sin esquema de color cae al default (oscuro).
import "./globals.css";

export default function GlobalError({
    reset,
}: {
    error: Error & { digest?: string };
    reset: () => void;
}) {
    return (
        <html lang="es" suppressHydrationWarning>
            <body className="antialiased font-sans">
                <div className="flex min-h-dvh flex-col items-center justify-center gap-6 bg-theme-canvas px-6 text-center">
                    <div className="flex flex-col items-center gap-2">
                        <h1 className="font-display text-2xl font-semibold text-theme-text">
                            Algo salió mal
                        </h1>
                        <p className="max-w-md text-sm text-theme-text-muted">
                            Ocurrió un error inesperado. Podés reintentar o
                            volver al inicio.
                        </p>
                    </div>
                    <div className="flex flex-wrap items-center justify-center gap-3">
                        <button
                            type="button"
                            onClick={reset}
                            className="rounded-md border border-theme-border-soft bg-theme-surface px-4 py-2 text-sm font-semibold text-theme-text transition-colors hover:bg-theme-surface-2"
                        >
                            Reintentar
                        </button>
                        {/* Reload completo a / a propósito: recupera desde un
                            estado raíz roto, no una navegación SPA. */}
                        {/* eslint-disable-next-line @next/next/no-html-link-for-pages */}
                        <a
                            href="/"
                            className="rounded-md bg-theme-accent px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-theme-accent-hover"
                        >
                            Volver al inicio
                        </a>
                    </div>
                </div>
            </body>
        </html>
    );
}
