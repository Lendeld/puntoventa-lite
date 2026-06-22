/**
 * Registro de handlers IPC para impresión.
 * Todos los handlers devuelven { ok: boolean, error?: string }.
 * NUNCA hace throw sin catch — errores con mensaje accionable en español.
 *
 * Llamar registerPrintingIpc(ipcMain) en el arranque de main.ts.
 */

import { ipcMain, BrowserWindow } from "electron";
import type { IpcMain } from "electron";
import { listarImpresoras, type PrinterInfo } from "./discovery";
import { obtenerConfigImpresion, guardarConfigImpresion, type ConfigImpresionLocal } from "./config";
import { buildTicket, buildTestPage, buildDrawerOnly, type TicketData, type PerfilImpresoraTicket } from "./escpos/ticket-renderer";
import { sendToPrinter } from "./transports/index";

export interface IpcResult<T = undefined> {
    ok: boolean;
    data?: T;
    error?: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function ok<T = undefined>(data?: T): IpcResult<any> {
    return { ok: true, data };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function err(msg: string): IpcResult<any> {
    return { ok: false, error: msg };
}

function catchMsg(e: unknown): string {
    if (e instanceof Error) return e.message;
    return String(e);
}

// ---------------------------------------------------------------------------
// Solicitud de impresión de ticket
// ---------------------------------------------------------------------------
export interface ImprimirTicketRequest {
    impresora: string;
    perfil: PerfilImpresoraTicket;
    ticket: TicketData;
    abrirGaveta: boolean;
    copias: number;
}

// ---------------------------------------------------------------------------
// Registro
// ---------------------------------------------------------------------------
export function registerPrintingIpc(ipc: IpcMain): void {

    // Listar impresoras disponibles
    ipc.handle("impresion:listar-impresoras", async (): Promise<IpcResult<PrinterInfo[]>> => {
        try {
            const lista = await listarImpresoras();
            return ok(lista);
        } catch (e) {
            return err(`Error al listar impresoras: ${catchMsg(e)}`);
        }
    });

    // Imprimir ticket (con copias)
    ipc.handle("impresion:imprimir-ticket", async (_event, req: ImprimirTicketRequest): Promise<IpcResult> => {
        try {
            const copias = Math.max(1, req.copias ?? 1);
            const aplicaCopia = req.ticket.aplicaCopiaClienteNegocio && copias >= 2;

            if (aplicaCopia) {
                // Copia para el cliente
                const bufCliente = buildTicket(req.ticket, req.perfil, req.abrirGaveta, "CLIENTE");
                await sendToPrinter(req.impresora, bufCliente);
                // Copia para el negocio (sin gaveta — ya se abrió en la primera)
                const bufNegocio = buildTicket(req.ticket, req.perfil, false, "NEGOCIO");
                await sendToPrinter(req.impresora, bufNegocio);
            } else {
                // N copias iguales
                const buf = buildTicket(req.ticket, req.perfil, req.abrirGaveta);
                for (let i = 0; i < copias; i++) {
                    await sendToPrinter(req.impresora, buf);
                }
            }

            return ok();
        } catch (e) {
            return err(`Error al imprimir ticket: ${catchMsg(e)}`);
        }
    });

    // Imprimir página de prueba
    ipc.handle("impresion:imprimir-prueba", async (_event, req: { impresora: string; perfil: PerfilImpresoraTicket }): Promise<IpcResult> => {
        try {
            const buf = buildTestPage(req.perfil);
            await sendToPrinter(req.impresora, buf);
            return ok();
        } catch (e) {
            return err(`Error al imprimir prueba: ${catchMsg(e)}`);
        }
    });

    // Abrir gaveta
    ipc.handle("impresion:abrir-gaveta", async (_event, req: { impresora: string; perfil: PerfilImpresoraTicket }): Promise<IpcResult> => {
        try {
            const buf = buildDrawerOnly(req.perfil);
            await sendToPrinter(req.impresora, buf);
            return ok();
        } catch (e) {
            return err(`Error al abrir gaveta: ${catchMsg(e)}`);
        }
    });

    // Obtener config
    ipc.handle("impresion:obtener-config", (): IpcResult<ConfigImpresionLocal> => {
        try {
            const cfg = obtenerConfigImpresion();
            return ok(cfg);
        } catch (e) {
            return err(`Error al leer configuración de impresión: ${catchMsg(e)}`);
        }
    });

    // Guardar config
    ipc.handle("impresion:guardar-config", (_event, config: ConfigImpresionLocal): IpcResult => {
        try {
            guardarConfigImpresion(config);
            return ok();
        } catch (e) {
            return err(`Error al guardar configuración de impresión: ${catchMsg(e)}`);
        }
    });

    // Imprimir HTML (fallback para impresoras sin ESC/POS)
    ipc.handle("impresion:imprimir-html", async (_event, req: { html: string; impresora?: string; anchoMm?: number }): Promise<IpcResult> => {
        try {
            const win = new BrowserWindow({
                show: false,
                webPreferences: {
                    contextIsolation: true,
                    nodeIntegration: false,
                    sandbox: true,
                },
            });
            await win.loadURL(`data:text/html;charset=utf-8,${encodeURIComponent(req.html)}`);

            await new Promise<void>((resolve, reject) => {
                win.webContents.print(
                    {
                        silent: true,
                        deviceName: req.impresora ?? "",
                        printBackground: true,
                    },
                    (success, failureReason) => {
                        win.destroy();
                        if (success) resolve();
                        else reject(new Error(failureReason ?? "Error al imprimir HTML"));
                    },
                );
            });

            return ok();
        } catch (e) {
            return err(`Error al imprimir HTML: ${catchMsg(e)}`);
        }
    });
}
