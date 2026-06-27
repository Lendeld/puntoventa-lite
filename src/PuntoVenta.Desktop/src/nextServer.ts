import { spawn, ChildProcess } from "node:child_process";
import path from "node:path";
import net from "node:net";
import fs from "node:fs";
import crypto from "node:crypto";
import { app, utilityProcess } from "electron";
import type { UtilityProcess } from "electron";
import { earlyLog } from "./log";

let serverProcess: ChildProcess | UtilityProcess | null = null;
let electronClientToken: string | null = null;

// Lo setea main.ts antes de spawn. Lo metemos en env de Next como
// ELECTRON_CLIENT_TOKEN, el middleware (proxy.ts) valida que cada
// request traiga ese mismo valor en header X-Electron-Token.
export function setElectronClientToken(token: string): void {
    electronClientToken = token;
}

// SESSION_SECRET per-instalacion. Mismo patron que JWT en apiServer.
// El middleware de Next exige >=32 chars. Persistido en userData,
// regenerable (invalida sesiones activas pero no rompe la app).
function getOrCreateSessionSecret(): string {
    const secretPath = path.join(app.getPath("userData"), "session-secret.txt");
    if (fs.existsSync(secretPath)) {
        const existing = fs.readFileSync(secretPath, "utf8").trim();
        if (existing.length >= 32) return existing;
    }
    const generated = crypto.randomBytes(48).toString("base64").replace(/[/+=]/g, "").slice(0, 64);
    fs.mkdirSync(path.dirname(secretPath), { recursive: true });
    fs.writeFileSync(secretPath, generated, { mode: 0o600 });
    earlyLog(`nextServer: SESSION_SECRET generada (${secretPath})`);
    return generated;
}

/**
 * Spawns the Next standalone server bundled with the desktop app and
 * returns the URL once it's ready. Only used in packaged (production)
 * builds — dev mode points at `next dev` directly.
 */
export async function startNextServer(apiUrl?: string): Promise<string> {
    if (serverProcess) {
        throw new Error("Next server already started");
    }

    const port = await findFreePort();
    const standaloneDir = resolveStandaloneDir();
    const serverEntry = path.join(standaloneDir, "server.js");
    earlyLog(`nextServer: port=${port}, standaloneDir=${standaloneDir}`);
    earlyLog(`nextServer: server.js existe? ${fs.existsSync(serverEntry)}, execPath=${process.execPath}`);

    const sessionSecret = getOrCreateSessionSecret();
    earlyLog(`nextServer: BASE_URL_API=${apiUrl ?? "<unset>"}, SESSION_SECRET.length=${sessionSecret.length}`);
    const nextEnv = {
        ...process.env,
        PORT: String(port),
        HOSTNAME: "127.0.0.1",
        NODE_ENV: "production",
        // Middleware Next exige >=32 chars; generado per-instalacion.
        SESSION_SECRET: sessionSecret,
        // Stack traces utilizables en logs (sin esto los stack van con
        // path del bundle minificado).
        NODE_OPTIONS: "--enable-source-maps",
        // Server actions del web consultan BASE_URL_API; apunta al child
        // .NET API local. Si no se pasa apiUrl (PV_SKIP_LOCAL_API=1),
        // dejamos el valor que venga del entorno (dev sin API local).
        ...(apiUrl ? { BASE_URL_API: apiUrl } : {}),
        // Token que el middleware exige en header X-Electron-Token.
        // Solo el BrowserWindow de Electron lo inyecta -> browser externo
        // accediendo al puerto local recibe 401.
        ...(electronClientToken ? { ELECTRON_CLIENT_TOKEN: electronClientToken } : {}),
    };

    // En macOS, `spawn(process.execPath, server.js)` puede aparecer como
    // otra app generica "exec" en el Dock. utilityProcess corre el server
    // como helper interno de Electron y evita esa entrada visual.
    if (app.isPackaged) {
        const proc = utilityProcess.fork(serverEntry, [], {
            env: nextEnv,
            cwd: standaloneDir,
            stdio: ["ignore", "pipe", "pipe"],
            serviceName: "PuntoVenta Next Server",
        });
        serverProcess = proc;
        wireUtilityProcess(proc, "nextServer");
    } else {
        const proc = spawn(process.execPath, [serverEntry], {
            env: {
                ...nextEnv,
                // Electron sets ELECTRON_RUN_AS_NODE so that the embedded
                // Node binary runs `server.js` instead of opening a window.
                ELECTRON_RUN_AS_NODE: "1",
            },
            cwd: standaloneDir,
            stdio: ["ignore", "pipe", "pipe"],
        });
        serverProcess = proc;
        wireChildProcess(proc, "nextServer");
    }

    earlyLog(`nextServer: waitForServer ${port} (30s)`);
    try {
        await waitForServer(port, 30_000);
    } catch (e) {
        earlyLog(`nextServer: waitForServer FALLO: ${(e as Error).message}`);
        throw e;
    }
    earlyLog(`nextServer: listening ${port}`);
    return `http://127.0.0.1:${port}`;
}

/**
 * Spawns `next dev` against the local Web project. Used by `pnpm dev` so
 * Electron orchesta API + Next juntos sin terminales separadas. Hot reload
 * sigue funcionando porque es el mismo `next dev` de siempre.
 */
export async function startNextDevServer(apiUrl?: string): Promise<string> {
    if (serverProcess) {
        throw new Error("Next dev server already started");
    }

    // Puerto distinto a 3000 para evitar colisión con `pnpm dev` del web
    // corriendo en otra terminal. Si 3000 está ocupado y bindeáramos ahí,
    // Electron cargaría el Next de nube en lugar del local.
    const port = 3030;
    const webDir = path.join(__dirname, "..", "..", "PuntoVenta.Web");
    const pnpmExecutable = process.platform === "win32" ? "pnpm.cmd" : "pnpm";

    // NEXT_DIST_DIR le pide al next.config.ts del web que use un .next-electron
    // separado del .next del Next dev normal (pnpm dev en otra terminal).
    // Sin esto, Next bloquea ambos por compartir lock en .next/.
    // Windows: spawnear pnpm.cmd exige shell:true en Node >=20.12 (sin él da
    // EINVAL — CVE-2024-27980 endureció el spawn de .cmd/.bat). El env va por
    // options.env (no por el shell) y los args son estáticos, así que no hay
    // superficie de inyección. En POSIX pnpm es binario y no necesita shell.
    serverProcess = spawn(pnpmExecutable, ["dev", "--port", String(port)], {
        env: {
            ...process.env,
            NEXT_DIST_DIR: ".next-electron",
            ...(apiUrl ? { BASE_URL_API: apiUrl } : {}),
        },
        cwd: webDir,
        stdio: ["ignore", "pipe", "pipe"],
        shell: process.platform === "win32",
    });

    serverProcess.stdout?.on("data", (chunk) => {
        process.stdout.write(`[next-dev] ${chunk}`);
    });
    serverProcess.stderr?.on("data", (chunk) => {
        process.stderr.write(`[next-dev] ${chunk}`);
    });
    serverProcess.on("exit", (code) => {
        console.log(`[next-dev] exited with code ${code}`);
        serverProcess = null;
    });

    // Next dev abre el TCP antes de estar listo para responder HTTP (compila
    // bajo demanda). HTTP poll asegura que el primer GET retorne 200/redirect
    // antes de que Electron cargue la URL.
    await waitForHttp(`http://localhost:${port}`, 120_000);
    // Next dev bindea localhost por defecto; usar `localhost` evita el warning
    // de allowedDevOrigins cuando Electron carga la URL.
    return `http://localhost:${port}`;
}

export async function stopNextServer(): Promise<void> {
    if (!serverProcess) return;
    return new Promise<void>((resolve) => {
        const proc = serverProcess!;
        serverProcess = null;
        if ("postMessage" in proc) {
            proc.once("exit", () => resolve());
        } else {
            proc.once("exit", () => resolve());
        }
        proc.kill();
        // Hard kill if it doesn't exit in 5s.
        setTimeout(() => {
            if ("killed" in proc) {
                if (!proc.killed) proc.kill("SIGKILL");
            } else if (proc.pid !== undefined) {
                proc.kill();
            }
            resolve();
        }, 5_000);
    });
}

function wireChildProcess(proc: ChildProcess, label: string): void {
    proc.stdout?.on("data", (chunk) => {
        earlyLog(`[next stdout] ${String(chunk).trimEnd()}`);
    });
    proc.stderr?.on("data", (chunk) => {
        earlyLog(`[next stderr] ${String(chunk).trimEnd()}`);
    });
    proc.on("error", (err) => {
        earlyLog(`${label}: spawn error ${err.message}`);
    });
    proc.on("exit", (code, signal) => {
        earlyLog(`${label}: exit code=${code} signal=${signal}`);
        serverProcess = null;
    });
}

function wireUtilityProcess(proc: UtilityProcess, label: string): void {
    proc.stdout?.on("data", (chunk) => {
        earlyLog(`[next stdout] ${String(chunk).trimEnd()}`);
    });
    proc.stderr?.on("data", (chunk) => {
        earlyLog(`[next stderr] ${String(chunk).trimEnd()}`);
    });
    proc.on("error", (type, location, report) => {
        earlyLog(`${label}: utility error type=${type} location=${location}\n${report}`);
    });
    proc.on("exit", (code) => {
        earlyLog(`${label}: exit code=${code}`);
        serverProcess = null;
    });
}

function resolveStandaloneDir(): string {
    // In packaged builds the standalone bundle ships under
    // `resources/app.asar.unpacked/standalone/` (or extraResources).
    // In unpacked dev it lives at `../PuntoVenta.Web/.next/standalone/`.
    if (app.isPackaged) {
        return path.join(process.resourcesPath, "standalone");
    }
    return path.join(__dirname, "..", "..", "PuntoVenta.Web", ".next", "standalone");
}

async function findFreePort(): Promise<number> {
    return new Promise<number>((resolve, reject) => {
        const server = net.createServer();
        server.unref();
        server.on("error", reject);
        server.listen(0, "127.0.0.1", () => {
            const address = server.address();
            if (typeof address === "object" && address) {
                const port = address.port;
                server.close(() => resolve(port));
            } else {
                reject(new Error("Failed to bind a free port"));
            }
        });
    });
}

async function waitForServer(port: number, timeoutMs: number): Promise<void> {
    const start = Date.now();
    while (Date.now() - start < timeoutMs) {
        const ok = await pingPort(port);
        if (ok) return;
        await delay(200);
    }
    throw new Error(`Next server did not start within ${timeoutMs}ms`);
}

function pingPort(port: number): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
        const socket = net.createConnection({ port, host: "127.0.0.1" });
        socket.once("connect", () => {
            socket.end();
            resolve(true);
        });
        socket.once("error", () => resolve(false));
    });
}

async function waitForHttp(url: string, timeoutMs: number): Promise<void> {
    const start = Date.now();
    let ultimaErr: unknown = null;
    while (Date.now() - start < timeoutMs) {
        try {
            // Algunos middlewares responden 3xx (redirect a /login); cualquier
            // status HTTP válido nos basta para saber que Next está listo.
            const res = await fetch(url, { redirect: "manual" });
            if (res.status > 0) return;
        } catch (e) {
            ultimaErr = e;
        }
        await delay(400);
    }
    throw new Error(`Next no respondió HTTP en ${timeoutMs}ms: ${(ultimaErr as Error)?.message ?? "timeout"}`);
}

function delay(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}
