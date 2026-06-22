import { spawn, ChildProcess } from "node:child_process";
import path from "node:path";
import net from "node:net";
import fs from "node:fs";
import crypto from "node:crypto";
import { app } from "electron";
import { earlyLog } from "./log";

let apiProcess: ChildProcess | null = null;

// JWT secret per-instalacion. Generada al primer arranque y persistida
// en userData/jwt-secret.txt. Si se borra, todos los logins se invalidan
// pero la app sigue funcionando (nuevo login regenera token).
function getOrCreateJwtSecret(): string {
    const secretPath = path.join(app.getPath("userData"), "jwt-secret.txt");
    if (fs.existsSync(secretPath)) {
        const existing = fs.readFileSync(secretPath, "utf8").trim();
        if (existing.length >= 32) return existing;
    }
    const generated = crypto.randomBytes(48).toString("base64").replace(/[/+=]/g, "").slice(0, 64);
    fs.mkdirSync(path.dirname(secretPath), { recursive: true });
    fs.writeFileSync(secretPath, generated, { mode: 0o600 });
    earlyLog(`apiServer: JWT secret generada (${secretPath})`);
    return generated;
}

/**
 * Spawns the .NET PuntoVenta.API process with SQLite. EF Core auto-migra
 * la DB al arrancar. Returns the URL once it accepts TCP connections.
 *
 * Layout:
 * - Packaged: resources/api/PuntoVenta.API(.exe) — published self-contained
 * - Dev:      ../PuntoVenta.API/bin/Debug/net10.0/PuntoVenta.API(.dll)
 */
export async function startApiServer(): Promise<string> {
    if (apiProcess) {
        throw new Error("API server already started");
    }

    earlyLog("apiServer.startApiServer: entrada");
    // Puerto fijo en dev para que `next dev` (que arranca en paralelo) pueda
    // tener BASE_URL_API hardcoded. Puerto libre en packaged.
    const port = app.isPackaged ? await findFreePort() : 5247;
    const apiInfo = resolveApiBinary();
    earlyLog(`apiServer: port=${port}, apiBinary=${apiInfo.kind}:${apiInfo.path}`);
    earlyLog(`apiServer: binario existe? ${fs.existsSync(apiInfo.path)}`);

    const dbPath = path.join(app.getPath("userData"), "puntoventa.db");
    const jwtSecret = getOrCreateJwtSecret();

    const env = {
        ...process.env,
        ASPNETCORE_URLS: `http://127.0.0.1:${port}`,
        ASPNETCORE_ENVIRONMENT: app.isPackaged ? "Production" : "Development",
        // Secrets van por env (no argv): no aparecen en `ps` ni en los logs
        // de spawn. .NET config los lee con separador `__`.
        Jwt__SecretKey: jwtSecret,
        Seed__Admin__Username: "admin",
        Seed__Admin__Password: "admin1234",
        Seed__Admin__RequiereCambioPassword: "true",
        // userData de Electron es TCC-friendly en mac (Application Support
        // del bundleId). El child .NET usa el mismo root para detectar que
        // corre como child de Electron (MacOSDockHider).
        PUNTOVENTA_DATA_ROOT: app.getPath("userData"),
        // Finder no hereda LANG/LC_*. Forzamos UTF-8 portable.
        LANG: process.env.LANG ?? "en_US.UTF-8",
        LC_ALL: process.env.LC_ALL ?? "en_US.UTF-8",
        // Mac: el child .NET aparece en Dock como "exec" porque es un
        // binario suelto sin Info.plist. Asociandolo al bundleId del
        // parent (__CFBundleIdentifier) macOS lo trata como parte de la
        // misma app y no crea entry en Dock.
        ...(process.platform === "darwin" ? { __CFBundleIdentifier: "com.puntoventalite.desktop" } : {}),
    } as NodeJS.ProcessEnv;

    // Config no-secreta por CLI: prioridad maxima en .NET config y no
    // depende de archivos (CWD puede diferir del ContentRoot al lanzar
    // via Electron). Los secrets van por env arriba.
    const cliConfig = [
        `--ConnectionStrings:DefaultConnection=Data Source=${dbPath}`,
        "--Jwt:Issuer=PuntoVenta.Desktop",
        "--Jwt:Audience=PuntoVenta.Desktop",
        "--Jwt:ExpiracionMinutos=480",
        "--Jwt:RefreshExpiracionDias=30",
        "--Logging:LogLevel:Default=Warning",
        "--Logging:LogLevel:Microsoft.AspNetCore=Warning",
        "--Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning",
        "--Logging:LogLevel:PuntoVenta=Information",
    ];

    const command = apiInfo.kind === "dll" ? "dotnet" : apiInfo.path;
    const args = apiInfo.kind === "dll" ? [apiInfo.path, ...cliConfig] : cliConfig;

    apiProcess = spawn(command, args, {
        env,
        cwd: path.dirname(apiInfo.path),
        stdio: ["ignore", "pipe", "pipe"],
    });

    earlyLog(`apiServer: spawn ${command} ${args.join(" ")}`);
    apiProcess.stdout?.on("data", (chunk) => {
        const line = String(chunk).trimEnd();
        earlyLog(`[api stdout] ${line}`);
    });
    apiProcess.stderr?.on("data", (chunk) => {
        const line = String(chunk).trimEnd();
        earlyLog(`[api stderr] ${line}`);
    });
    apiProcess.on("error", (err) => {
        earlyLog(`apiServer: spawn error ${err.message}`);
    });
    apiProcess.on("exit", (code, signal) => {
        earlyLog(`apiServer: process exit code=${code} signal=${signal}`);
        apiProcess = null;
    });

    // SQLite + EF migrate es rapido (< 5s en frio). 30s es suficiente
    // holgura para hardware lento; si tarda mas es un bug real.
    earlyLog(`apiServer: waitForServer ${port} (30s)`);
    await waitForServer(port, 30_000);
    earlyLog(`apiServer: listening ${port}`);
    return `http://127.0.0.1:${port}`;
}

export async function stopApiServer(): Promise<void> {
    if (!apiProcess) return;
    return new Promise<void>((resolve) => {
        const proc = apiProcess!;
        apiProcess = null;
        proc.once("exit", () => resolve());
        proc.kill();
        setTimeout(() => {
            if (!proc.killed) {
                proc.kill("SIGKILL");
            }
            resolve();
        }, 8_000);
    });
}

type ApiBinary =
    | { kind: "exe"; path: string }
    | { kind: "dll"; path: string };

function resolveApiBinary(): ApiBinary {
    if (app.isPackaged) {
        const exeName = process.platform === "win32" ? "PuntoVenta.API.exe" : "PuntoVenta.API";
        // Intentar primero el wrapper .app de mac (creado en builds anteriores
        // con publish-desktop.sh). Si no existe, usar el binario directo.
        if (process.platform === "darwin") {
            const wrappedPath = path.join(process.resourcesPath, "api", "PuntoVentaApi.app", "Contents", "MacOS", exeName);
            if (fs.existsSync(wrappedPath)) return { kind: "exe", path: wrappedPath };
        }
        const exePath = path.join(process.resourcesPath, "api", exeName);
        if (fs.existsSync(exePath)) return { kind: "exe", path: exePath };
        const dllPath = path.join(process.resourcesPath, "api", "PuntoVenta.API.dll");
        return { kind: "dll", path: dllPath };
    }
    // Dev: use the API project's last Debug build.
    const repoRoot = path.join(__dirname, "..", "..", "..");
    const dllPath = path.join(
        repoRoot,
        "src",
        "PuntoVenta.API",
        "bin",
        "Debug",
        "net10.0",
        "PuntoVenta.API.dll",
    );
    return { kind: "dll", path: dllPath };
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
        await delay(300);
    }
    throw new Error(`API did not start within ${timeoutMs}ms`);
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

function delay(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}
