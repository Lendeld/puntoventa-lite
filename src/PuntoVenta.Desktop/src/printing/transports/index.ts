/**
 * Router de transports — enruta por designación de impresora.
 *
 * "tcp://..." | "socket://..." | IP numérica | hostname con punto → TCP raw
 * "serial://..."                                                   → serial
 * cualquier otro                                                   → cola del sistema (CUPS/winspool)
 */

import { parseTcpEndpoint, sendTcp } from "./tcp";
import { parseSerialEndpoint, sendSerial } from "./serial";
import { sendSistema } from "./sistema";

export async function sendToPrinter(printerName: string, data: Buffer): Promise<void> {
    if (!printerName || !printerName.trim()) {
        throw new Error("Nombre de impresora vacío. Selecciona una impresora en la configuración.");
    }

    // 1. Serial
    const serialEp = parseSerialEndpoint(printerName);
    if (serialEp) {
        await sendSerial(serialEp.path, serialEp.baud, data);
        return;
    }

    // 2. TCP directo (tcp://, socket://, IP numérica, hostname con punto)
    const tcpEp = parseTcpEndpoint(printerName);
    if (tcpEp) {
        await sendTcp(tcpEp.host, tcpEp.port, data);
        return;
    }

    // 3. Cola del sistema (CUPS en Unix/macOS, winspool en Windows)
    await sendSistema(printerName, data);
}
