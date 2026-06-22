/**
 * Preload script de Electron.
 * Expone window.pulpoImpresion al renderer vía contextBridge.
 * contextIsolation: true — ningún módulo Node llega directo al renderer.
 *
 * Contrato con el frontend (src/PuntoVenta.Web/src/lib/types/impresion-bridge.d.ts):
 * los IpcResult internos se desenvuelven acá — listarImpresoras/obtenerConfig
 * devuelven el valor directo; las operaciones devuelven { ok, error? }.
 */

import { contextBridge, ipcRenderer } from "electron";
import type { TicketData, PerfilImpresoraTicket } from "./printing/escpos/ticket-renderer";
import type { ConfigImpresionLocal } from "./printing/config";
import type { PrinterInfo } from "./printing/discovery";
import type { IpcResult, ImprimirTicketRequest } from "./printing/ipc";

// Re-exportar tipos para que el frontend los pueda usar
export type { TicketData, PerfilImpresoraTicket, ConfigImpresionLocal, PrinterInfo, IpcResult };

export interface ResultadoImpresion {
    ok: boolean;
    error?: string;
}

async function invokeOperacion(channel: string, ...args: unknown[]): Promise<ResultadoImpresion> {
    const result = (await ipcRenderer.invoke(channel, ...args)) as IpcResult;
    return { ok: result.ok, error: result.error };
}

const pulpoImpresion = {
    async listarImpresoras(): Promise<PrinterInfo[]> {
        const result = (await ipcRenderer.invoke(
            "impresion:listar-impresoras",
        )) as IpcResult<PrinterInfo[]>;
        if (!result.ok) {
            throw new Error(result.error ?? "Error al listar impresoras");
        }
        return result.data ?? [];
    },

    imprimirTicket(req: ImprimirTicketRequest): Promise<ResultadoImpresion> {
        return invokeOperacion("impresion:imprimir-ticket", req);
    },

    imprimirPrueba(req: { impresora: string; perfil: PerfilImpresoraTicket }): Promise<ResultadoImpresion> {
        return invokeOperacion("impresion:imprimir-prueba", req);
    },

    abrirGaveta(req: { impresora: string; perfil: PerfilImpresoraTicket }): Promise<ResultadoImpresion> {
        return invokeOperacion("impresion:abrir-gaveta", req);
    },

    async obtenerConfig(): Promise<ConfigImpresionLocal | null> {
        const result = (await ipcRenderer.invoke(
            "impresion:obtener-config",
        )) as IpcResult<ConfigImpresionLocal>;
        return result.ok ? (result.data ?? null) : null;
    },

    async guardarConfig(cfg: ConfigImpresionLocal): Promise<{ ok: boolean }> {
        const result = (await ipcRenderer.invoke("impresion:guardar-config", cfg)) as IpcResult;
        return { ok: result.ok };
    },

    imprimirHtml(req: { html: string; impresora?: string; anchoMm?: number }): Promise<ResultadoImpresion> {
        return invokeOperacion("impresion:imprimir-html", req);
    },
};

contextBridge.exposeInMainWorld("pulpoImpresion", pulpoImpresion);

// ---------------------------------------------------------------------------
// pulpoBackup — bridge para backup/restore de la base de datos
// ---------------------------------------------------------------------------
export interface ResultadoBackup {
    ok: boolean;
    error?: string;
    /** Presente en restore exitoso: indica que la app debe reiniciarse. */
    requiereReinicio?: boolean;
}

interface ResultadoRestoreData {
    requiereReinicio?: boolean;
}

const pulpoBackup = {
    async elegirDestino(): Promise<string | null> {
        const result = (await ipcRenderer.invoke("backup:elegir-destino")) as IpcResult<string | null>;
        return result.ok ? (result.data ?? null) : null;
    },

    async elegirOrigen(): Promise<string | null> {
        const result = (await ipcRenderer.invoke("backup:elegir-origen")) as IpcResult<string | null>;
        return result.ok ? (result.data ?? null) : null;
    },

    async restaurar(rutaOrigen: string, token: string): Promise<ResultadoBackup> {
        const result = (await ipcRenderer.invoke("backup:restaurar", rutaOrigen, token)) as IpcResult<ResultadoRestoreData>;
        return {
            ok: result.ok,
            error: result.error,
            requiereReinicio: result.data?.requiereReinicio,
        };
    },

    /** Relanza la app completa (app.relaunch + app.exit). Llamar tras restore exitoso. */
    reiniciarApp(): void {
        void ipcRenderer.invoke("backup:reiniciar-app");
    },
};

contextBridge.exposeInMainWorld("pulpoBackup", pulpoBackup);

// Declaración de tipo global para el renderer (usada en el frontend)
declare global {
    interface Window {
        pulpoImpresion: typeof pulpoImpresion;
        pulpoBackup: typeof pulpoBackup;
    }
}
