/**
 * Transport de sistema — port de UnixRawPrinter.cs + WindowsRawPrinter.cs.
 *
 * darwin/linux: port de UnixRawPrinter
 *   - IP/hostname → TCP directo
 *   - Cola CUPS con URI socket:// → resuelve con lpstat -v (LANG=C) → TCP
 *   - Fallback → lp -d <cola> -o raw stdin
 *
 * win32: port de WindowsRawPrinter
 *   - PowerShell con Add-Type del RawPrinterHelper (OpenPrinter/WritePrinter
 *     via winspool.dll, datatype RAW). Escribe data en archivo temporal.
 */

import { exec, execFile, spawn } from "node:child_process";
import { promisify } from "node:util";
import os from "node:os";
import path from "node:path";
import fs from "node:fs";
import { parseTcpEndpoint, sendTcp } from "./tcp";

const execAsync = promisify(exec);
const execFileAsync = promisify(execFile);

// ---------------------------------------------------------------------------
// Cache de resolución URI CUPS (equiv. a ConcurrentDictionary del C#)
// ---------------------------------------------------------------------------
interface ResolvedEndpoint {
    isTcp: boolean;
    host: string;
    port: number;
}

const uriCache = new Map<string, ResolvedEndpoint>();

// ---------------------------------------------------------------------------
// Unix / macOS
// ---------------------------------------------------------------------------

/** Envía bytes a una impresora del sistema en Unix/macOS. */
export async function sendUnix(printerName: string, data: Buffer): Promise<void> {
    if (!printerName) throw new Error("Nombre de impresora requerido.");
    if (!data.length) throw new Error("No hay datos para imprimir.");

    // 1. Si el nombre ya es IP/hostname → TCP directo
    const direct = parseTcpEndpoint(printerName);
    if (direct) {
        await sendTcp(direct.host, direct.port, data);
        return;
    }

    // 2. Cola CUPS con URI socket:// → resolver y usar TCP
    let resolved = uriCache.get(printerName);
    if (!resolved) {
        resolved = await resolveCupsUri(printerName);
        uriCache.set(printerName, resolved);
    }

    if (resolved.isTcp) {
        try {
            await sendTcp(resolved.host, resolved.port, data);
            return;
        } catch {
            // URI cachada inválida → limpiar y re-resolver una vez
            uriCache.delete(printerName);
            const fresh = await resolveCupsUri(printerName);
            uriCache.set(printerName, fresh);
            if (fresh.isTcp) {
                await sendTcp(fresh.host, fresh.port, data);
                return;
            }
        }
    }

    // 3. Fallback: lp -d <cola> -o raw stdin
    await sendViaLp(printerName, data);
}

/** Resuelve URI de CUPS con lpstat -v (LANG=C). Port de ResolveAsync del C#. */
async function resolveCupsUri(printerName: string): Promise<ResolvedEndpoint> {
    const NOT_TCP: ResolvedEndpoint = { isTcp: false, host: "", port: 0 };
    try {
        const { stdout } = await execAsync(`lpstat -v "${printerName}"`, {
            env: { ...process.env, LANG: "C", LC_ALL: "C" },
            timeout: 5000,
        });
        // Formato: "device for <name>: socket://192.168.1.100:9100"
        const colonIdx = stdout.indexOf(":");
        if (colonIdx < 0) return NOT_TCP;

        const uri = stdout.slice(colonIdx + 1).trim();
        if (uri.toLowerCase().startsWith("socket://")) {
            const ep = parseTcpEndpoint(uri);
            if (ep) return { isTcp: true, host: ep.host, port: ep.port };
        }
        return NOT_TCP;
    } catch {
        return NOT_TCP;
    }
}

/** Envía via lp -d <cola> -o raw leyendo datos por stdin. */
function sendViaLp(printerName: string, data: Buffer): Promise<void> {
    return new Promise((resolve, reject) => {
        const child = spawn("lp", ["-d", printerName, "-o", "raw"], {
            stdio: ["pipe", "pipe", "pipe"],
        });

        let stderr = "";
        child.stderr.on("data", (chunk: Buffer) => {
            stderr += chunk.toString();
        });

        child.on("close", (code) => {
            if (code !== 0) {
                reject(new Error(`lp falló con código ${code}: ${stderr}`));
            } else {
                resolve();
            }
        });

        child.on("error", (err) => {
            reject(
                new Error(
                    `No se pudo ejecutar 'lp'. Verifica que CUPS esté instalado. Detalle: ${err.message}`,
                ),
            );
        });

        child.stdin.write(data);
        child.stdin.end();
    });
}

// ---------------------------------------------------------------------------
// Windows
// ---------------------------------------------------------------------------

/** Script PowerShell inline con winspool RAW. */
const PS_RAW_PRINTER_SCRIPT = `
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class RawPrinterHelper {
    [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);
    [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern bool ClosePrinter(IntPtr hPrinter);
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public class DOCINFO {
        [MarshalAs(UnmanagedType.LPTStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPTStr)] public string pOutputFile;
        [MarshalAs(UnmanagedType.LPTStr)] public string pDataType;
    }
    [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern Int32 StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFO di);
    [DllImport("winspool.drv", SetLastError=true)] public static extern bool EndDocPrinter(IntPtr hPrinter);
    [DllImport("winspool.drv", SetLastError=true)] public static extern bool StartPagePrinter(IntPtr hPrinter);
    [DllImport("winspool.drv", SetLastError=true)] public static extern bool EndPagePrinter(IntPtr hPrinter);
    [DllImport("winspool.drv", SetLastError=true)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);
    public static void SendRawData(string printerName, string filePath) {
        IntPtr hPrinter;
        if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            throw new Exception("OpenPrinter falló. Win32=" + Marshal.GetLastWin32Error());
        try {
            var di = new DOCINFO { pDocName = "PuntoVenta Ticket", pDataType = "RAW" };
            if (StartDocPrinter(hPrinter, 1, di) == 0)
                throw new Exception("StartDocPrinter falló. Win32=" + Marshal.GetLastWin32Error());
            try {
                if (!StartPagePrinter(hPrinter))
                    throw new Exception("StartPagePrinter falló. Win32=" + Marshal.GetLastWin32Error());
                byte[] data = System.IO.File.ReadAllBytes(filePath);
                IntPtr pBytes = Marshal.AllocCoTaskMem(data.Length);
                try {
                    Marshal.Copy(data, 0, pBytes, data.Length);
                    int written;
                    if (!WritePrinter(hPrinter, pBytes, data.Length, out written))
                        throw new Exception("WritePrinter falló. Win32=" + Marshal.GetLastWin32Error());
                    if (written != data.Length)
                        throw new Exception("WritePrinter escribió " + written + " de " + data.Length + " bytes.");
                } finally { Marshal.FreeCoTaskMem(pBytes); }
                EndPagePrinter(hPrinter);
            } finally { EndDocPrinter(hPrinter); }
        } finally { ClosePrinter(hPrinter); }
    }
}
"@
[RawPrinterHelper]::SendRawData($args[0], $args[1])
`;

/** Envía bytes a una impresora Windows vía winspool RAW (PowerShell). */
export async function sendWindows(printerName: string, data: Buffer): Promise<void> {
    if (!printerName) throw new Error("Nombre de impresora requerido.");
    if (!data.length) throw new Error("No hay datos para imprimir.");

    // Script y bytes en archivos temporales. Se invoca con execFile + -File
    // (sin shell, sin comillas) para que $args ligue printerName/tmpFile y el
    // script multilínea no se rompa en cmd.exe.
    const stamp = `${process.pid}-${Date.now()}`;
    const tmpFile = path.join(os.tmpdir(), `pv-ticket-${stamp}.bin`);
    const scriptFile = path.join(os.tmpdir(), `pv-print-${stamp}.ps1`);
    try {
        fs.writeFileSync(tmpFile, data);
        fs.writeFileSync(scriptFile, PS_RAW_PRINTER_SCRIPT, "utf8");
        await execFileAsync(
            "powershell",
            [
                "-NoProfile",
                "-NonInteractive",
                "-ExecutionPolicy", "Bypass",
                "-File", scriptFile,
                printerName,
                tmpFile,
            ],
            { timeout: 15000, windowsHide: true },
        );
    } catch (e) {
        const detalle = e instanceof Error ? e.message : String(e);
        throw new Error(
            `No se pudo imprimir en «${printerName}» (winspool RAW). ` +
            `Verifica que la impresora esté encendida, en línea y acepte impresión sin formato (RAW). Detalle: ${detalle}`,
        );
    } finally {
        try { fs.unlinkSync(tmpFile); } catch { /* ignorar */ }
        try { fs.unlinkSync(scriptFile); } catch { /* ignorar */ }
    }
}

// ---------------------------------------------------------------------------
// Router por plataforma
// ---------------------------------------------------------------------------

/** Envía al sistema según la plataforma actual. */
export async function sendSistema(printerName: string, data: Buffer): Promise<void> {
    const plat = os.platform();
    if (plat === "win32") {
        await sendWindows(printerName, data);
    } else {
        await sendUnix(printerName, data);
    }
}
