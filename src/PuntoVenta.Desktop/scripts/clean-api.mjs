// Borra el estado local del desktop (SQLite DB + secrets + logs) en el
// userData de Electron para forzar un arranque limpio.
//
// userData por plataforma:
//   mac:   ~/Library/Application Support/<name>
//   win:   %APPDATA%/<name>
//   linux: ~/.config/<name>
//
// Electron arma userData con el `name` de package.json (puntoventa-lite-desktop);
// el productName es "Punto Venta Lite". Cubrimos ambos por las dudas.

import { execSync } from "node:child_process";
import { existsSync, rmSync } from "node:fs";
import path from "node:path";
import os from "node:os";

// Archivos que conforman el estado local SQLite del desktop.
const STATE_FILES = [
    "puntoventa.db",
    "puntoventa.db-wal",
    "puntoventa.db-shm",
    "jwt-secret.txt",
    "session-secret.txt",
    "api.log",
    "impresion.json",
];

const userDataPaths = resolveUserDataPaths();
console.log(`[clean-api] userData candidates: ${userDataPaths.join(", ")}`);

for (const dataRoot of userDataPaths) {
    if (!existsSync(dataRoot)) {
        console.log(`[clean-api] skip ${dataRoot} (not present)`);
        continue;
    }
    console.log(`[clean-api] cleaning ${dataRoot}`);
    for (const filename of STATE_FILES) {
        const target = path.join(dataRoot, filename);
        if (existsSync(target)) {
            rmSync(target, { force: true });
            console.log(`[clean-api]   removed ${target}`);
        }
    }
}

console.log("[clean-api] dotnet clean PuntoVenta.API");
try {
    execSync(
        "dotnet clean ../PuntoVenta.API/PuntoVenta.API.csproj --nologo",
        { stdio: "inherit" },
    );
} catch {
    console.warn("[clean-api] dotnet clean fallo (sin .NET SDK o sin build previo)");
}

console.log("[clean-api] done.");

/**
 * Devuelve los posibles paths de userData de Electron para esta plataforma.
 * Tanto el name (puntoventa-lite-desktop) como el productName (Punto Venta Lite)
 * son validos segun como Electron resuelva getName().
 */
function resolveUserDataPaths() {
    const appNames = ["puntoventa-lite-desktop", "Punto Venta Lite"];
    if (process.platform === "win32") {
        const appData = process.env.APPDATA ?? path.join(os.homedir(), "AppData", "Roaming");
        return appNames.map((n) => path.join(appData, n));
    }
    if (process.platform === "darwin") {
        return appNames.map((n) => path.join(os.homedir(), "Library", "Application Support", n));
    }
    // Linux
    return appNames.map((n) => path.join(os.homedir(), ".config", n));
}
