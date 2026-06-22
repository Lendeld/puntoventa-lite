/**
 * Registro de handlers IPC para backup y restore de la base de datos.
 * Canales:
 *   backup:elegir-destino → dialog.showSaveDialog → ruta o null
 *   backup:elegir-origen  → dialog.showOpenDialog  → ruta o null
 *   backup:restaurar      → swap del archivo SQLite + reinicio del API
 *
 * Llamar registerBackupIpc(ipcMain, { stopApiServer, startApiServer })
 * desde main.ts antes de createWindow.
 *
 * NUNCA hace throw sin catch — errores con mensaje accionable en español.
 */

import { ipcMain, dialog, app } from "electron";
import type { IpcMain } from "electron";
import fs from "node:fs";
import path from "node:path";
import { earlyLog } from "../log";
import type { IpcResult } from "../printing/ipc";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function ok<T = undefined>(data?: T): IpcResult<any> {
    return { ok: true, data };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function err(msg: string): IpcResult<any> {
    return { ok: false, error: msg };
}

// Error que además pide reiniciar la app. Se usa tras un rollback: el API se relanzó
// (posiblemente en otro puerto efímero) y Next seguiría apuntando al puerto viejo, así
// que hay que relanzar la app completa para volver a un estado consistente.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function errReinicio(msg: string): IpcResult<any> {
    return { ok: false, error: msg, data: { requiereReinicio: true } };
}

function catchMsg(e: unknown): string {
    if (e instanceof Error) return e.message;
    return String(e);
}

/** Nombre por defecto del archivo de backup con fecha. */
function defaultBackupName(): string {
    const now = new Date();
    const yyyy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, "0");
    const dd = String(now.getDate()).padStart(2, "0");
    const HH = String(now.getHours()).padStart(2, "0");
    const min = String(now.getMinutes()).padStart(2, "0");
    return `puntoventa-backup-${yyyy}${mm}${dd}-${HH}${min}.db`;
}

// ---------------------------------------------------------------------------
// Tipo de resultado expuesto al renderer
// ---------------------------------------------------------------------------
export interface ResultadoBackup {
    ok: boolean;
    error?: string;
}

// ---------------------------------------------------------------------------
// Dependencias inyectadas desde main.ts
// ---------------------------------------------------------------------------
export interface BackupIpcDependencies {
    stopApiServer: () => Promise<void>;
    startApiServer: () => Promise<string>;
    /** URL base del API child en ejecución (ej. http://127.0.0.1:5247) o undefined si no hay API local. */
    getApiBaseUrl: () => string | undefined;
}

// Tipo extendido para el resultado del restore
interface ResultadoRestore extends ResultadoBackup {
    requiereReinicio?: boolean;
}

// ---------------------------------------------------------------------------
// Registro
// ---------------------------------------------------------------------------
export function registerBackupIpc(ipc: IpcMain, deps: BackupIpcDependencies): void {

    // Relanza la app completa (API + Next) de forma limpia.
    // Necesario tras restore porque en build empaquetado el API arranca en
    // puerto efímero: el nuevo spawn elige un puerto diferente al original y
    // Next/apiClient seguirían apuntando al puerto muerto si no se relanza.
    // El setTimeout da tiempo al renderer de recibir la respuesta IPC antes
    // de que el proceso muera.
    ipc.handle("backup:reiniciar-app", (): void => {
        earlyLog("backup:reiniciar-app — app.relaunch() + app.exit(0)");
        setTimeout(() => {
            app.relaunch();
            app.exit(0);
        }, 300);
    });

    // Diálogo "Guardar como" para elegir destino del backup
    ipc.handle("backup:elegir-destino", async (): Promise<IpcResult<string | null>> => {
        try {
            const result = await dialog.showSaveDialog({
                title: "Guardar respaldo de base de datos",
                defaultPath: defaultBackupName(),
                filters: [{ name: "Base de datos SQLite", extensions: ["db"] }],
                properties: ["createDirectory", "showOverwriteConfirmation"],
            });
            if (result.canceled || !result.filePath) {
                return ok(null);
            }
            return ok(result.filePath);
        } catch (e) {
            return err(`Error al abrir diálogo de destino: ${catchMsg(e)}`);
        }
    });

    // Diálogo "Abrir" para elegir el archivo de backup a restaurar
    ipc.handle("backup:elegir-origen", async (): Promise<IpcResult<string | null>> => {
        try {
            const result = await dialog.showOpenDialog({
                title: "Seleccionar respaldo de base de datos",
                filters: [{ name: "Base de datos SQLite", extensions: ["db"] }],
                properties: ["openFile"],
            });
            if (result.canceled || result.filePaths.length === 0) {
                return ok(null);
            }
            return ok(result.filePaths[0]);
        } catch (e) {
            return err(`Error al abrir diálogo de origen: ${catchMsg(e)}`);
        }
    });

    // Swap del archivo SQLite con reinicio del API.
    // Antes del swap, main consume el token de capacidad contra el backend (mientras el
    // API sigue vivo): así el swap nativo NO puede dispararse sin una validación previa
    // real (PIN + permiso + versión) — cierra el bypass del bridge.
    ipc.handle("backup:restaurar", async (_event, rutaOrigen: string, token: string): Promise<IpcResult<ResultadoRestore>> => {
        const userData = app.getPath("userData");
        const dbPath = path.join(userData, "puntoventa.db");
        const dbBakPath = path.join(userData, "puntoventa.db.bak");
        const dbWal = path.join(userData, "puntoventa.db-wal");
        const dbShm = path.join(userData, "puntoventa.db-shm");

        earlyLog(`backup:restaurar inicio — origen=${rutaOrigen}`);

        // Verificar que el origen existe antes de parar la API
        if (!fs.existsSync(rutaOrigen)) {
            return err("El archivo de respaldo no existe o no es accesible.");
        }

        // 0. Consumir el token de autorización contra el backend (API aún vivo).
        const apiBaseUrl = deps.getApiBaseUrl();
        if (apiBaseUrl) {
            try {
                const resp = await fetch(`${apiBaseUrl}/backup/consumir-token`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ token, ruta: rutaOrigen }),
                });
                if (!resp.ok) {
                    earlyLog(`backup:restaurar — token rechazado (HTTP ${resp.status}), abortando`);
                    return err("Autorización de restauración inválida o expirada. Vuelve a validar el respaldo.");
                }
            } catch (e) {
                earlyLog(`backup:restaurar — fallo al consumir token: ${catchMsg(e)}`);
                return err(`No se pudo verificar la autorización de restauración: ${catchMsg(e)}`);
            }
        } else {
            // Sin API local (dev/skipLocalApi): no hay backend contra el cual validar.
            earlyLog("backup:restaurar — sin API base URL (dev/skipLocalApi), se omite verificación de token");
        }

        // 1. Parar el API (libera lock SQLite + checkpoint WAL al cerrar)
        earlyLog("backup:restaurar — stopApiServer");
        try {
            await deps.stopApiServer();
        } catch (e) {
            earlyLog(`backup:restaurar — stopApiServer FALLO: ${catchMsg(e)}`);
            // Continuar de todas formas — en dev puede no haber API corriendo
        }

        // Limpiar -wal/-shm residuales (el close debió hacer checkpoint)
        for (const f of [dbWal, dbShm]) {
            try {
                if (fs.existsSync(f)) fs.unlinkSync(f);
            } catch (e) {
                earlyLog(`backup:restaurar — no pudo borrar ${f}: ${catchMsg(e)}`);
            }
        }

        // 2. Backup defensivo del archivo vivo → .bak
        earlyLog(`backup:restaurar — backup defensivo → ${dbBakPath}`);
        let hizoBak = false;
        try {
            if (fs.existsSync(dbPath)) {
                fs.copyFileSync(dbPath, dbBakPath);
                hizoBak = true;
            }
        } catch (e) {
            earlyLog(`backup:restaurar — no pudo crear .bak: ${catchMsg(e)}`);
            // No bloqueante: si no existe la DB actual, no hay nada que proteger
        }

        // 3. Copiar el backup elegido sobre la DB viva
        earlyLog(`backup:restaurar — copyFile ${rutaOrigen} → ${dbPath}`);
        try {
            fs.copyFileSync(rutaOrigen, dbPath);
        } catch (e) {
            earlyLog(`backup:restaurar — copyFile FALLO: ${catchMsg(e)}`);
            // Intentar revertir desde .bak
            await revertirDesdeBAk(dbPath, dbBakPath, hizoBak, deps);
            return errReinicio(`Error al copiar el respaldo. Se revirtió al estado anterior. Error: ${catchMsg(e)}`);
        }

        // 4. Rearrancar el API
        earlyLog("backup:restaurar — startApiServer");
        try {
            await deps.startApiServer();
        } catch (e) {
            earlyLog(`backup:restaurar — startApiServer FALLO: ${catchMsg(e)}`);
            await revertirDesdeBAk(dbPath, dbBakPath, hizoBak, deps);
            return errReinicio(`El respaldo no pudo iniciarse. Se revirtió al estado anterior. Error: ${catchMsg(e)}`);
        }

        // 5. Éxito: borrar .bak y notificar al renderer que debe reiniciar la app.
        // NO navegamos a /login desde main: el renderer muestra feedback y luego
        // dispara backup:reiniciar-app → app.relaunch() + app.exit(0).
        // Esto garantiza que en build empaquetado el nuevo proceso del API arranca
        // en un puerto efímero fresco y Next recibe BASE_URL_API actualizado al
        // inicio del nuevo ciclo de vida, eliminando la inconsistencia de puertos.
        earlyLog("backup:restaurar — OK, borrando .bak, solicitando reinicio de app");
        try {
            if (hizoBak && fs.existsSync(dbBakPath)) fs.unlinkSync(dbBakPath);
        } catch { /* ignorar — no crítico */ }

        return ok({ requiereReinicio: true });
    });
}

// ---------------------------------------------------------------------------
// Rollback helper
// ---------------------------------------------------------------------------
async function revertirDesdeBAk(
    dbPath: string,
    dbBakPath: string,
    hizoBak: boolean,
    deps: BackupIpcDependencies,
): Promise<void> {
    if (!hizoBak || !fs.existsSync(dbBakPath)) {
        earlyLog("backup:revertir — no hay .bak disponible");
        return;
    }
    earlyLog(`backup:revertir — copiando .bak → ${dbPath}`);
    try {
        fs.copyFileSync(dbBakPath, dbPath);
        fs.unlinkSync(dbBakPath);
        await deps.startApiServer();
        earlyLog("backup:revertir — API relanzado OK desde .bak");
    } catch (e) {
        earlyLog(`backup:revertir — FALLO: ${catchMsg(e)}`);
    }
}
