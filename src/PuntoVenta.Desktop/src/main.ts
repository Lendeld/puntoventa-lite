import { earlyLog } from "./log";
import os from "node:os";
import crypto from "node:crypto";

// Token compartido entre Electron y Next. Inyectado en cada request
// del BrowserWindow via webRequest hook; Next middleware lo valida.
// Sin esto, cualquiera con el puerto random podria abrir la web local
// en su browser.
const ELECTRON_CLIENT_TOKEN = crypto.randomBytes(32).toString("base64url");

earlyLog(`main.ts loaded, argv=${JSON.stringify(process.argv)}, env.NODE_ENV=${process.env.NODE_ENV ?? "unset"}`);

// Finder no hereda env del shell del usuario (PATH minimo, LANG/HOME/USER
// unset). Sin esto el spawn de Next/.NET puede fallar porque los childs
// necesitan HOME para resolver paths.
if (process.platform === "darwin") {
    process.env.HOME ||= os.homedir();
    process.env.USER ||= os.userInfo().username;
    process.env.LANG ||= "en_US.UTF-8";
    process.env.LC_ALL ||= "en_US.UTF-8";
    const homebrewPaths = ["/opt/homebrew/bin", "/usr/local/bin"];
    const currentPath = process.env.PATH ?? "";
    const missing = homebrewPaths.filter((p) => !currentPath.includes(p));
    if (missing.length) {
        process.env.PATH = [...missing, currentPath].filter(Boolean).join(":");
    }
}

import { app, BrowserWindow, ipcMain, shell } from "electron";
import path from "node:path";
import { startNextServer, startNextDevServer, stopNextServer, setElectronClientToken } from "./nextServer";
import { startApiServer, stopApiServer } from "./apiServer";
import { registerPrintingIpc } from "./printing/ipc";
import { registerBackupIpc } from "./backup/ipc";

earlyLog(`imports OK, isPackaged=${app.isPackaged}, resourcesPath=${process.resourcesPath}`);
setElectronClientToken(ELECTRON_CLIENT_TOKEN);

const isDev = process.env.NODE_ENV === "development";
// PV_SKIP_LOCAL_API=1 corta el spawn del .NET API. Util en dev cuando
// quieres correr la API manualmente en otra terminal.
const skipLocalApi = process.env.PV_SKIP_LOCAL_API === "1";
// PV_SKIP_NEXT_SPAWN=1 corta el spawn de Next y carga PV_DEV_URL directo.
// Util si ya tienes `pnpm dev` corriendo en otra terminal.
const skipNextSpawn = process.env.PV_SKIP_NEXT_SPAWN === "1";
const DEV_URL = process.env.PV_DEV_URL ?? "http://localhost:3000";

let mainWindow: BrowserWindow | null = null;

async function createWindow() {
    earlyLog(`createWindow: start, isDev=${isDev}, skipLocalApi=${skipLocalApi}`);
    // Ruta al preload compilado (dist/preload.js)
    const preloadPath = path.join(__dirname, "preload.js");

    mainWindow = new BrowserWindow({
        width: 1280,
        height: 800,
        minWidth: 1024,
        minHeight: 700,
        title: "Punto Venta Lite",
        icon: getIconPath(),
        autoHideMenuBar: true,
        backgroundColor: "#1a1a1a",
        webPreferences: {
            contextIsolation: true,
            nodeIntegration: false,
            sandbox: false, // preload necesita acceso a Node (contextBridge)
            preload: preloadPath,
        },
    });

    // Captura console + fallos de red del renderer al log unificado. Sin
    // esto, errores de login (que pasan en el browser) se pierden si no
    // hay DevTools abierto.
    mainWindow.webContents.on("console-message", (_e, level, message, line, sourceId) => {
        const niveles = ["debug", "info", "warn", "error"];
        earlyLog(`[renderer ${niveles[level] ?? level}] ${message} (${sourceId}:${line})`);
    });
    mainWindow.webContents.on("did-fail-load", (_e, code, desc, url) => {
        earlyLog(`[renderer did-fail-load] ${code} ${desc} url=${url}`);
    });
    mainWindow.webContents.session.webRequest.onCompleted((details) => {
        if (details.statusCode >= 400) {
            earlyLog(`[renderer http ${details.statusCode}] ${details.method} ${details.url}`);
        }
    });

    // Inyecta token en cada request salida del BrowserWindow. Next
    // middleware lo valida -> requests sin token (browser externo) 401.
    mainWindow.webContents.session.webRequest.onBeforeSendHeaders((details, callback) => {
        callback({
            requestHeaders: {
                ...details.requestHeaders,
                "X-Electron-Token": ELECTRON_CLIENT_TOKEN,
            },
        });
    });

    mainWindow.webContents.setWindowOpenHandler(({ url }) => {
        // Loopback (Next local) -> nueva BrowserWindow misma sesion para
        // que herede cookies + token via webRequest. PDFs y rutas internas
        // entran por aca. Externo -> default browser.
        try {
            const parsed = new URL(url);
            if (parsed.hostname === "127.0.0.1" || parsed.hostname === "localhost") {
                return {
                    action: "allow",
                    overrideBrowserWindowOptions: {
                        width: 900,
                        height: 1100,
                        autoHideMenuBar: true,
                        webPreferences: { contextIsolation: true, sandbox: true },
                    },
                };
            }
        } catch { /* URL invalida, fall-through */ }
        if (url.startsWith("http://") || url.startsWith("https://")) {
            void shell.openExternal(url);
        }
        return { action: "deny" };
    });

    mainWindow.on("closed", () => {
        mainWindow = null;
    });

    earlyLog("createWindow: BrowserWindow listo, llamando startApiServer");
    // Flujo: API child (que migra SQLite con EF al arrancar) → Next.
    let apiUrl: string | undefined;
    try {
        apiUrl = skipLocalApi ? undefined : await startApiServer();
        currentApiUrl = apiUrl;
        earlyLog(`startApiServer OK, apiUrl=${apiUrl ?? "skipped"}`);
    } catch (e) {
        earlyLog(`startApiServer FALLO: ${(e as Error).message}\n${(e as Error).stack}`);
        throw e;
    }

    earlyLog("cargarAppNext: start");
    await cargarAppNext(apiUrl);
    earlyLog("cargarAppNext: OK");
}

async function cargarAppNext(apiUrl: string | undefined) {
    if (!mainWindow) return;
    const url = isDev
        ? skipNextSpawn ? DEV_URL : await startNextDevServer(apiUrl)
        : await startNextServer(apiUrl);
    await mainWindow.loadURL(url);
    if (isDev || process.env.PV_DEBUG === "1") {
        mainWindow.webContents.openDevTools({ mode: "detach" });
    }
}

function getIconPath(): string {
    const resourcesDir = app.isPackaged
        ? process.resourcesPath
        : path.join(__dirname, "..", "resources");
    return process.platform === "win32"
        ? path.join(resourcesDir, "icon.ico")
        : path.join(resourcesDir, "icon.png");
}

process.on("uncaughtException", (err) => {
    earlyLog(`uncaughtException: ${err.message}\n${err.stack}`);
});
process.on("unhandledRejection", (reason) => {
    earlyLog(`unhandledRejection: ${String(reason)}`);
});

// Registrar IPC de impresion al arranque (antes de createWindow)
registerPrintingIpc(ipcMain);

// URL base del API child en ejecución. La usa el IPC de backup para consumir el
// token de restauración contra el backend antes del swap. Se actualiza en createWindow.
let currentApiUrl: string | undefined;

// Registrar IPC de backup/restore al arranque (antes de createWindow)
// Tras restore exitoso el handler devuelve requiereReinicio:true y el
// renderer invoca backup:reiniciar-app → app.relaunch()+app.exit(0),
// garantizando puertos API + Next consistentes en el nuevo ciclo.
registerBackupIpc(ipcMain, {
    stopApiServer,
    // Envuelto para que cada arranque del API desde el IPC (swap o rollback) actualice
    // currentApiUrl: en build empaquetado el puerto es efímero y puede cambiar.
    startApiServer: async () => {
        const url = await startApiServer();
        currentApiUrl = url;
        return url;
    },
    getApiBaseUrl: () => currentApiUrl,
});

earlyLog("registrando app.whenReady");
app.whenReady().then(() => {
    earlyLog("app.whenReady fired");
    return createWindow();
}).catch((err) => {
    earlyLog(`whenReady/createWindow FALLO: ${err?.message}\n${err?.stack}`);
    void Promise.allSettled([stopNextServer(), stopApiServer()]).finally(() => {
        app.exit(1);
    });
});

app.on("window-all-closed", () => {
    void Promise.allSettled([stopNextServer(), stopApiServer()]).finally(() => {
        if (process.platform !== "darwin") {
            app.quit();
        }
    });
});

app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
        void createWindow();
    }
});

app.on("before-quit", () => {
    void stopNextServer();
    void stopApiServer();
});
