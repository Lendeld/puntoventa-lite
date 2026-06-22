// Prepara standalone-staged/ copiando el output del build Next standalone
// y renombrando node_modules -> _modules (electron-builder filtra node_modules
// de extraResources; after-pack.mjs lo restaura dentro del bundle).
//
// Correr despues de `pnpm build:next`.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const DESKTOP_DIR = path.resolve(__dirname, "..");
const WEB_DIR = path.resolve(DESKTOP_DIR, "..", "PuntoVenta.Web");
const STANDALONE_SRC = path.join(WEB_DIR, ".next", "standalone");
const STAGED = path.join(DESKTOP_DIR, "standalone-staged");

console.log("[stage-next] inicio");
console.log(`[stage-next] WEB_DIR:        ${WEB_DIR}`);
console.log(`[stage-next] STANDALONE_SRC: ${STANDALONE_SRC}`);
console.log(`[stage-next] STAGED:         ${STAGED}`);

// Verificar que el build de Next existe.
const serverEntry = path.join(STANDALONE_SRC, "server.js");
if (!fs.existsSync(serverEntry)) {
    console.error(`[stage-next] ERROR: no existe ${serverEntry}`);
    console.error("[stage-next] Corre `pnpm build:next` primero.");
    process.exit(1);
}

// Inyectar assets estaticos: .next/static y public.
// Next standalone no incluye estos dirs — hay que copiarlos manualmente.
const staticSrc = path.join(WEB_DIR, ".next", "static");
const staticDst = path.join(STANDALONE_SRC, ".next", "static");
if (fs.existsSync(staticSrc)) {
    if (fs.existsSync(staticDst)) fs.rmSync(staticDst, { recursive: true, force: true });
    fs.cpSync(staticSrc, staticDst, { recursive: true });
    console.log(`[stage-next] copiado .next/static -> ${staticDst}`);
}

const publicSrc = path.join(WEB_DIR, "public");
const publicDst = path.join(STANDALONE_SRC, "public");
if (fs.existsSync(publicSrc)) {
    if (fs.existsSync(publicDst)) fs.rmSync(publicDst, { recursive: true, force: true });
    fs.cpSync(publicSrc, publicDst, { recursive: true });
    console.log(`[stage-next] copiado public -> ${publicDst}`);
}

// Mirror: borrar STAGED y copiar STANDALONE_SRC -> STAGED.
if (fs.existsSync(STAGED)) {
    fs.rmSync(STAGED, { recursive: true, force: true });
    console.log(`[stage-next] borrado STAGED previo`);
}
fs.cpSync(STANDALONE_SRC, STAGED, { recursive: true });
console.log(`[stage-next] copiado standalone -> ${STAGED}`);

// Sanity check: next debe estar en node_modules del standalone
// (pnpm nodeLinker hoisted => archivos reales, no symlinks vacios).
const nextPkg = path.join(STAGED, "node_modules", "next", "package.json");
if (!fs.existsSync(nextPkg)) {
    console.error(`[stage-next] ERROR: sanity check fallo — no existe ${nextPkg}`);
    console.error("[stage-next] El standalone no tiene node_modules/next. Verifica que WEB_DIR use nodeLinker hoisted.");
    process.exit(1);
}
console.log("[stage-next] sanity check OK (next/package.json existe)");

// Renombrar node_modules -> _modules.
// electron-builder filtra directorios llamados "node_modules" de
// extraResources incluso con filter explicito. after-pack.mjs los
// restaura al nombre correcto dentro del bundle final.
const nodeModules = path.join(STAGED, "node_modules");
const underscoreModules = path.join(STAGED, "_modules");
if (fs.existsSync(underscoreModules)) {
    fs.rmSync(underscoreModules, { recursive: true, force: true });
}
fs.renameSync(nodeModules, underscoreModules);
console.log(`[stage-next] renombrado node_modules -> _modules`);

console.log("[stage-next] done.");
