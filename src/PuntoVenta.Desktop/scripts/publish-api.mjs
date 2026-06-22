// Publica PuntoVenta.API self-contained en src/PuntoVenta.Desktop/resources/api/.
// Usado por electron-builder.extraResources para bundlear la API en el MSI/DMG.
//
// Convenciones:
// - --target-rid=<rid> o env TARGET_RID fuerzan el runtime (default: host).
//   .NET cross-publica self-contained a cualquier RID (baja el runtime pack
//   por NuGet), así que win-x64 desde mac funciona sin toolchain nativa.
// - Configuración Release por defecto

import { execSync } from "node:child_process";
import { existsSync, rmSync, mkdirSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const desktopDir = path.resolve(__dirname, "..");
const repoRoot = path.resolve(desktopDir, "..", "..");
const apiProject = path.join(repoRoot, "src", "PuntoVenta.API", "PuntoVenta.API.csproj");
const outputDir = path.join(desktopDir, "resources", "api");

const args = process.argv.slice(2);
const ridArg = args.find((a) => a.startsWith("--target-rid="))?.split("=")[1];
const config = args.find((a) => a.startsWith("--configuration="))?.split("=")[1] ?? "Release";

// Precedencia: --target-rid > env TARGET_RID > RID del host. La env var deja
// que `dist:win` propague win-x64 a través de `pnpm build` -> `pnpm publish:api`.
const rid = ridArg ?? process.env.TARGET_RID ?? detectRid();
console.log(`[publish-api] rid=${rid} configuration=${config}`);
console.log(`[publish-api] output=${outputDir}`);

if (existsSync(outputDir)) {
    rmSync(outputDir, { recursive: true, force: true });
}
mkdirSync(outputDir, { recursive: true });

// Versión: del tag CalVer en release (env VERSION) o flag --version=. En dev
// queda vacío -> usa el default de Directory.Build.props (0.0.0-dev).
const version =
    args.find((a) => a.startsWith("--version="))?.split("=")[1] ??
    process.env.VERSION ??
    "";

const cmd = [
    "dotnet publish",
    `"${apiProject}"`,
    `-c ${config}`,
    `-r ${rid}`,
    "--self-contained true",
    "-p:PublishSingleFile=false",
    "-p:PublishReadyToRun=false",
    ...(version ? [`-p:Version=${version}`] : []),
    `-o "${outputDir}"`,
].join(" ");

console.log(`[publish-api] ${cmd}`);
execSync(cmd, { stdio: "inherit", cwd: repoRoot });
console.log(`[publish-api] OK`);

function detectRid() {
    const platform = process.platform;
    const arch = process.arch;
    if (platform === "win32") return arch === "arm64" ? "win-arm64" : "win-x64";
    if (platform === "darwin") return arch === "arm64" ? "osx-arm64" : "osx-x64";
    if (platform === "linux") return arch === "arm64" ? "linux-arm64" : "linux-x64";
    throw new Error(`Plataforma no soportada: ${platform}`);
}
