// Junta los meta-<os>.json de cada build en un único releases/latest.json,
// que es lo que lee el landing para versión + links de descarga + sha256.
//
// Uso: node assemble-manifest.mjs <version> <date> <artifactsDir>
//   El artifactsDir contiene subcarpetas (una por artifact descargado),
//   cada una con su meta-<os>.json.

import { readdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const [version, date, artifactsDir] = process.argv.slice(2);

if (!version || !date || !artifactsDir) {
  console.error("Uso: assemble-manifest.mjs <version> <date> <artifactsDir>");
  process.exit(1);
}

const assets = [];

for (const entry of readdirSync(artifactsDir)) {
  const sub = join(artifactsDir, entry);
  if (!statSync(sub).isDirectory()) continue;
  for (const f of readdirSync(sub)) {
    if (/^meta-.*\.json$/.test(f)) {
      assets.push(JSON.parse(readFileSync(join(sub, f), "utf8")));
    }
  }
}

if (assets.length === 0) {
  console.error(`No se encontró ningún meta-*.json en ${artifactsDir}`);
  process.exit(1);
}

// Orden estable por OS para que el JSON sea determinista.
assets.sort((a, b) => a.os.localeCompare(b.os));

const manifest = { version, date, assets };
writeFileSync("latest.json", JSON.stringify(manifest, null, 2));
console.log("latest.json:", JSON.stringify(manifest, null, 2));
