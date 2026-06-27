// Arranca electron forzando que NO esté ELECTRON_RUN_AS_NODE en el entorno.
// VS Code (y otros runners de Electron) suelen heredar esa variable al
// terminal integrado, lo que hace que `require("electron")` devuelva un path
// string en vez de los APIs (app/BrowserWindow undefined).

import { spawn } from "node:child_process";
import { createRequire } from "node:module";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const desktopDir = path.resolve(__dirname, "..");

// Resolvemos el binario real (electron.exe) vía el paquete, NO el shim
// .bin/electron.cmd: Node >=20.12 en Windows rechaza spawnear .cmd/.bat sin
// shell:true (EINVAL, CVE-2024-27980). El .exe directo evita el EINVAL y mantiene
// el sin-shell (electronBin es ruta fija y args estáticos: cero superficie de inyección).
const require = createRequire(import.meta.url);
const electronBin = require("electron");

const { ELECTRON_RUN_AS_NODE, ...env } = process.env;

const child = spawn(electronBin, ["."], {
    env: { ...env, NODE_ENV: "development" },
    cwd: desktopDir,
    stdio: "inherit",
});

child.on("exit", (code) => process.exit(code ?? 0));
