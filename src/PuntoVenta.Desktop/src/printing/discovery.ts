/**
 * Listar impresoras disponibles del sistema.
 *
 * darwin/linux: lpstat -p (LANG=C) → lista de colas CUPS.
 * win32: PowerShell Get-Printer | ConvertTo-Json.
 * serial: serialport.list() si el módulo carga.
 */

import { exec } from "node:child_process";
import { promisify } from "node:util";
import os from "node:os";

const execAsync = promisify(exec);

export interface PrinterInfo {
    nombre: string;
    origen: "sistema" | "serial";
    esDefault: boolean;
}

export async function listarImpresoras(): Promise<PrinterInfo[]> {
    const results: PrinterInfo[] = [];

    const sistema = await listarSistema();
    results.push(...sistema);

    const seriales = await listarSerial();
    results.push(...seriales);

    return results;
}

// ---------------------------------------------------------------------------
// Sistema
// ---------------------------------------------------------------------------

async function listarSistema(): Promise<PrinterInfo[]> {
    const plat = os.platform();
    if (plat === "win32") {
        return listarWindows();
    }
    return listarUnix();
}

async function listarUnix(): Promise<PrinterInfo[]> {
    try {
        const { stdout } = await execAsync("lpstat -p", {
            env: { ...process.env, LANG: "C", LC_ALL: "C" },
            timeout: 5000,
        });
        // Formato línea: "printer <name> is idle. enabled since ..."
        // O: "printer <name> disabled since ..."
        const printers: PrinterInfo[] = [];
        for (const line of stdout.split("\n")) {
            const m = line.match(/^printer\s+(\S+)/);
            if (m?.[1]) {
                printers.push({ nombre: m[1], origen: "sistema", esDefault: false });
            }
        }

        // Detectar default con lpstat -d
        try {
            const { stdout: defOut } = await execAsync("lpstat -d", {
                env: { ...process.env, LANG: "C", LC_ALL: "C" },
                timeout: 3000,
            });
            const defMatch = defOut.match(/system default destination:\s+(\S+)/);
            if (defMatch?.[1]) {
                const defName = defMatch[1];
                for (const p of printers) {
                    if (p.nombre === defName) p.esDefault = true;
                }
            }
        } catch { /* sin default */ }

        return printers;
    } catch {
        return [];
    }
}

async function listarWindows(): Promise<PrinterInfo[]> {
    try {
        const { stdout } = await execAsync(
            "powershell -NoProfile -Command \"Get-Printer | Select-Object Name,Default | ConvertTo-Json\"",
            { timeout: 10000 },
        );

        const raw = JSON.parse(stdout.trim());
        // PowerShell puede retornar objeto o array
        const arr: Array<{ Name: string; Default: boolean }> = Array.isArray(raw) ? raw : [raw];
        return arr.map((p) => ({
            nombre: p.Name,
            origen: "sistema",
            esDefault: p.Default === true,
        }));
    } catch {
        return [];
    }
}

// ---------------------------------------------------------------------------
// Serial
// ---------------------------------------------------------------------------

async function listarSerial(): Promise<PrinterInfo[]> {
    try {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-assignment
        const mod: any = await (eval('import("serialport")') as Promise<unknown>);
        const fn = mod.SerialPort?.list ?? mod.default?.SerialPort?.list ?? mod.list;
        if (typeof fn !== "function") return [];

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const ports: any[] = await fn();
        return ports.map((p) => ({
            nombre: `serial://${p.path as string}`,
            origen: "serial",
            esDefault: false,
        }));
    } catch {
        return [];
    }
}
