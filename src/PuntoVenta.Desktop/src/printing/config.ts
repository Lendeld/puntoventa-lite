/**
 * Persistencia de ConfigImpresionLocal en userData/impresion.json.
 * Sigue el mismo estilo que src/config.ts (loadConfig/cached pattern).
 */

import fs from "node:fs";
import path from "node:path";
import { app } from "electron";

export interface ConfigImpresionLocal {
    impresora: string | null;
    perfilClave: string | null;
    abrirGavetaAlCobrar: boolean;
    copias: number;
}

const DEFAULTS: ConfigImpresionLocal = {
    impresora: null,
    perfilClave: null,
    abrirGavetaAlCobrar: false,
    copias: 1,
};

function configPath(): string {
    return path.join(app.getPath("userData"), "impresion.json");
}

export function obtenerConfigImpresion(): ConfigImpresionLocal {
    try {
        const filePath = configPath();
        if (fs.existsSync(filePath)) {
            const raw = fs.readFileSync(filePath, "utf8");
            const parsed = JSON.parse(raw) as Partial<ConfigImpresionLocal>;
            return {
                impresora: parsed.impresora ?? DEFAULTS.impresora,
                perfilClave: parsed.perfilClave ?? DEFAULTS.perfilClave,
                abrirGavetaAlCobrar: parsed.abrirGavetaAlCobrar ?? DEFAULTS.abrirGavetaAlCobrar,
                copias: typeof parsed.copias === "number" && parsed.copias > 0 ? parsed.copias : DEFAULTS.copias,
            };
        }
    } catch { /* ignorar, retornar defaults */ }
    return { ...DEFAULTS };
}

export function guardarConfigImpresion(config: ConfigImpresionLocal): void {
    const filePath = configPath();
    const dir = path.dirname(filePath);
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }
    fs.writeFileSync(filePath, JSON.stringify(config, null, 2), "utf8");
}
