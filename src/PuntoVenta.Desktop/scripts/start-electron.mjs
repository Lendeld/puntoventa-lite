// Arranca electron forzando que NO esté ELECTRON_RUN_AS_NODE en el entorno.
// VS Code (y otros runners de Electron) suelen heredar esa variable al
// terminal integrado, lo que hace que `require("electron")` devuelva un path
// string en vez de los APIs (app/BrowserWindow undefined).

import { spawn } from "node:child_process";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const desktopDir = path.resolve(__dirname, "..");
const electronBin = path.join(
    desktopDir,
    "node_modules",
    ".bin",
    process.platform === "win32" ? "electron.cmd" : "electron",
);

const { ELECTRON_RUN_AS_NODE, ...env } = process.env;

// Sin shell — electronBin ya incluye .cmd en Windows y Node lo ejecuta directo.
// Usar shell propagaría env del shell y abre superficie de inyección.
const child = spawn(electronBin, ["."], {
    env: { ...env, NODE_ENV: "development" },
    cwd: desktopDir,
    stdio: "inherit",
});

child.on("exit", (code) => process.exit(code ?? 0));
