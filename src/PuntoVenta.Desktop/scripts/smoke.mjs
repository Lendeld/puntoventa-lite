// Smoke test post-pack: verifica el layout del bundle y lanza el binario
// .NET API contra una DB SQLite temporal para confirmar que arranca y
// responde /health.
//
// Prerequisito: `pnpm pack` ya corrido (release/ con --dir).

import fs from "node:fs";
import path from "node:path";
import os from "node:os";
import { spawn } from "node:child_process";
import crypto from "node:crypto";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const DESKTOP_DIR = path.resolve(__dirname, "..");
const RELEASE_DIR = path.join(DESKTOP_DIR, "release");
const SMOKE_PORT = 5599;
const HEALTH_URL = `http://127.0.0.1:${SMOKE_PORT}/health`;
const POLL_TIMEOUT_MS = 30_000;
const POLL_INTERVAL_MS = 400;

let apiProc = null;
let tempDir = null;

async function main() {
    console.log("[smoke] inicio");

    // --- Localizar resources dir del bundle ---
    const resourcesDir = findResourcesDir();
    console.log(`[smoke] resources: ${resourcesDir}`);

    // --- Verificar layout ---
    checkLayout(resourcesDir);
    console.log("[smoke] layout OK");

    // --- Crear dir temporal para DB ---
    tempDir = fs.mkdtempSync(path.join(os.tmpdir(), "pv-smoke-"));
    const dbPath = path.join(tempDir, "smoke.db");
    console.log(`[smoke] tempDir: ${tempDir}`);

    // --- Encontrar binario API ---
    const apiBin = findApiBinary(resourcesDir);
    console.log(`[smoke] api binary: ${apiBin}`);

    const jwtSecret = crypto.randomBytes(48).toString("base64").replace(/[/+=]/g, "").slice(0, 64);
    const cliArgs = [
        `--ConnectionStrings:DefaultConnection=Data Source=${dbPath}`,
        "--Jwt:Issuer=SmokeTest",
        "--Jwt:Audience=SmokeTest",
        "--Jwt:ExpiracionMinutos=60",
        "--Jwt:RefreshExpiracionDias=1",
        "--Logging:LogLevel:Default=Warning",
        "--Logging:LogLevel:Microsoft.AspNetCore=Warning",
        "--Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning",
    ];

    // Secrets por env, no argv — mismo patron que apiServer.ts.
    const env = {
        ...process.env,
        ASPNETCORE_URLS: `http://127.0.0.1:${SMOKE_PORT}`,
        ASPNETCORE_ENVIRONMENT: "Production",
        Jwt__SecretKey: jwtSecret,
        Seed__Admin__Username: "admin",
        Seed__Admin__Password: "admin1234",
        Seed__Admin__RequiereCambioPassword: "true",
    };

    console.log(`[smoke] spawn ${apiBin}`);
    apiProc = spawn(apiBin, cliArgs, {
        env,
        cwd: path.dirname(apiBin),
        stdio: ["ignore", "pipe", "pipe"],
    });
    apiProc.stdout?.on("data", (c) => process.stdout.write(`[smoke api] ${c}`));
    apiProc.stderr?.on("data", (c) => process.stderr.write(`[smoke api] ${c}`));
    apiProc.on("exit", (code) => {
        if (apiProc !== null) {
            // Salio antes de que terminara el poll
            console.error(`[smoke] API salio inesperadamente con code=${code}`);
        }
    });

    // --- Poll /health ---
    console.log(`[smoke] polling ${HEALTH_URL} (${POLL_TIMEOUT_MS}ms timeout)`);
    await pollHealth();

    console.log("[smoke] OK");
    cleanup(0);
}

function findResourcesDir() {
    const candidates = [
        // mac arm64
        path.join(RELEASE_DIR, "mac-arm64", "Punto Venta.app", "Contents", "Resources"),
        // mac x64
        path.join(RELEASE_DIR, "mac", "Punto Venta.app", "Contents", "Resources"),
        // win
        path.join(RELEASE_DIR, "win-unpacked", "resources"),
        // linux
        path.join(RELEASE_DIR, "linux-unpacked", "resources"),
    ];
    for (const c of candidates) {
        if (fs.existsSync(c)) return c;
    }
    console.error("[smoke] ERROR: no se encontro el resources dir del bundle.");
    console.error(`[smoke] Buscado en:\n  ${candidates.join("\n  ")}`);
    console.error("[smoke] Corre `pnpm pack` primero.");
    process.exit(1);
}

function checkLayout(resourcesDir) {
    const serverJs = path.join(resourcesDir, "standalone", "server.js");
    if (!fs.existsSync(serverJs)) {
        fail(`layout: falta standalone/server.js en ${resourcesDir}`);
    }

    const nextPkg = path.join(resourcesDir, "standalone", "node_modules", "next", "package.json");
    if (!fs.existsSync(nextPkg)) {
        fail(`layout: falta standalone/node_modules/next/package.json — afterPack no restauro _modules?`);
    }
}

function findApiBinary(resourcesDir) {
    const apiDir = path.join(resourcesDir, "api");
    if (!fs.existsSync(apiDir)) {
        fail(`layout: falta directorio api/ en ${resourcesDir}`);
    }

    if (process.platform === "darwin") {
        // Intentar primero wrapper .app (builds anteriores con publish-desktop.sh)
        const wrapped = path.join(apiDir, "PuntoVentaApi.app", "Contents", "MacOS", "PuntoVenta.API");
        if (fs.existsSync(wrapped)) return wrapped;
        // Binario directo (builds F4+)
        const direct = path.join(apiDir, "PuntoVenta.API");
        if (fs.existsSync(direct)) return direct;
        fail(`layout: no se encontro el binario API en ${apiDir}`);
    }

    if (process.platform === "win32") {
        const exe = path.join(apiDir, "PuntoVenta.API.exe");
        if (fs.existsSync(exe)) return exe;
        fail(`layout: no se encontro PuntoVenta.API.exe en ${apiDir}`);
    }

    // Linux
    const bin = path.join(apiDir, "PuntoVenta.API");
    if (fs.existsSync(bin)) return bin;
    fail(`layout: no se encontro el binario API en ${apiDir}`);
}

async function pollHealth() {
    const start = Date.now();
    let lastErr = null;
    while (Date.now() - start < POLL_TIMEOUT_MS) {
        try {
            // HTTP loopback deliberado: el API del smoke escucha solo en 127.0.0.1.
            // nosemgrep: typescript.react.security.react-insecure-request.react-insecure-request
            const res = await fetch(HEALTH_URL);
            if (res.status === 200) return;
            lastErr = new Error(`HTTP ${res.status}`);
        } catch (e) {
            lastErr = e;
        }
        await delay(POLL_INTERVAL_MS);
    }
    fail(`/health no respondio 200 en ${POLL_TIMEOUT_MS}ms. Ultimo error: ${lastErr?.message ?? "timeout"}`);
}

function fail(msg) {
    console.error(`[smoke] ERROR: ${msg}`);
    cleanup(1);
}

function cleanup(exitCode) {
    if (apiProc) {
        const proc = apiProc;
        apiProc = null;
        try { proc.kill(); } catch { /* ya muerto */ }
    }
    if (tempDir && fs.existsSync(tempDir)) {
        try { fs.rmSync(tempDir, { recursive: true, force: true }); } catch { /* noop */ }
    }
    process.exit(exitCode);
}

function delay(ms) {
    return new Promise((r) => setTimeout(r, ms));
}

main().catch((err) => {
    console.error(`[smoke] excepcion: ${err?.message}\n${err?.stack}`);
    cleanup(1);
});
