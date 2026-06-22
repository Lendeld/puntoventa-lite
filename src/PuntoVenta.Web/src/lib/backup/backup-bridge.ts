"use client";

import type { PulpoBackup } from "@lib/types/backup-bridge";

/**
 * Devuelve true solo cuando se ejecuta dentro de la app de escritorio Electron
 * que expone `window.pulpoBackup` via contextBridge.
 * En browser normal la propiedad es siempre undefined, por lo que es seguro
 * llamarlo en ambos entornos.
 */
export function esAppEscritorio(): boolean {
    return (
        typeof window !== "undefined" &&
        "pulpoBackup" in window &&
        typeof window.pulpoBackup !== "undefined"
    );
}

/**
 * Devuelve el bridge de backup o null cuando no corre dentro de la app Electron.
 * Los callers deben verificar null antes de usar cualquier método del bridge.
 */
export function getBackupBridge(): PulpoBackup | null {
    if (
        typeof window === "undefined" ||
        !("pulpoBackup" in window) ||
        typeof window.pulpoBackup === "undefined"
    ) {
        return null;
    }
    return window.pulpoBackup as PulpoBackup;
}
