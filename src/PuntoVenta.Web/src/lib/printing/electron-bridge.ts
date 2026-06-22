"use client";

import type { PulpoImpresion } from "@lib/types/impresion-bridge";

/**
 * Returns true only when running inside the Electron desktop app that exposes
 * `window.pulpoImpresion` via contextBridge.  In a normal browser the property
 * is always undefined, so this is safe to call in both environments.
 */
export function esAppEscritorio(): boolean {
    return (
        typeof window !== "undefined" &&
        "pulpoImpresion" in window &&
        typeof window.pulpoImpresion !== "undefined"
    );
}

/**
 * Returns the typed bridge or null when not running inside the Electron app.
 * Callers must check for null before using any bridge method.
 */
export function getBridge(): PulpoImpresion | null {
    if (!esAppEscritorio()) return null;
    return window.pulpoImpresion as PulpoImpresion;
}
