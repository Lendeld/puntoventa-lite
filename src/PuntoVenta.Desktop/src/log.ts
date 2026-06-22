import fs from "node:fs";

// Logger unificado a /tmp/puntoventa-early.log (path fijo, no depende
// de userData ni de electron inicializado). Asi todo aparece en un
// archivo aunque la app crashee antes de inicializar Electron.
//
// Gated por PV_DEBUG=1: arranque normal queda silencioso. Para
// diagnosticar, exportar PV_DEBUG=1 o usar `PV_DEBUG=1 open -a "Punto Venta Lite"`.
const DEBUG = process.env.PV_DEBUG === "1";

export function earlyLog(msg: string): void {
    if (!DEBUG) return;
    try {
        fs.appendFileSync(
            "/tmp/puntoventa-early.log",
            `[${new Date().toISOString()}] ${msg}\n`,
        );
    } catch {
        // ignore
    }
}
