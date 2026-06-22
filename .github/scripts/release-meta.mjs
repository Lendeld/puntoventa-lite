// Calcula sha256 + tamaño del instalador recién empaquetado y escribe
// release/meta-<asset_os>.json. Corre en el runner de build (cwd = src/PuntoVenta.Desktop).
//
// Uso: node release-meta.mjs <asset_os> <label> <version> <pattern>
//   asset_os : id de OS que consume el landing (win | mac-arm)
//   label    : etiqueta legible (ej. "Windows")
//   version  : versión del release (CalVer, ej. 2026.06.21)
//   pattern  : glob del instalador dentro de release/ (ej. "PuntoVentaLite-*-x64.exe")

import { createHash } from "node:crypto";
import { readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const [assetOs, label, version, pattern] = process.argv.slice(2);

if (!assetOs || !label || !version || !pattern) {
  console.error("Uso: release-meta.mjs <asset_os> <label> <version> <pattern>");
  process.exit(1);
}

const releaseDir = join(process.cwd(), "release");

// Glob simple -> regex (solo soporta '*').
const re = new RegExp(
  "^" + pattern.replace(/[.+?^${}()|[\]\\]/g, "\\$&").replace(/\*/g, ".*") + "$",
);

const file = readdirSync(releaseDir).find((f) => re.test(f));
if (!file) {
  console.error(`No se encontró instalador que matchee "${pattern}" en ${releaseDir}`);
  console.error("Contenido:", readdirSync(releaseDir).join(", "));
  process.exit(1);
}

const full = join(releaseDir, file);
const buf = readFileSync(full);
const sha256 = createHash("sha256").update(buf).digest("hex");
const bytes = statSync(full).size;
const size = `${Math.round(bytes / (1024 * 1024))} MB`;

const meta = {
  os: assetOs,
  label,
  filename: file,
  key: `releases/${version}/${file}`,
  sha256,
  size,
};

writeFileSync(join(releaseDir, `meta-${assetOs}.json`), JSON.stringify(meta, null, 2));
console.log("meta:", JSON.stringify(meta, null, 2));
