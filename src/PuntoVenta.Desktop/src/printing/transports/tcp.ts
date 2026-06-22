/**
 * Transport TCP raw — port de TcpRawPrinter.cs.
 * Envía bytes directamente al socket de la impresora (puerto 9100 JetDirect).
 * Sin CUPS: ~100ms de latencia vs ~3s con cola del sistema.
 */

import net from "node:net";

export interface TcpEndpoint {
    host: string;
    port: number;
}

const DEFAULT_PORT = 9100;
const CONNECT_TIMEOUT_MS = 5000;

/**
 * Parsea designaciones TCP de impresoras.
 * Port de TcpRawPrinter.TryParseEndpoint del C#.
 *
 * Formatos soportados:
 *   "tcp://192.168.1.100:9100"
 *   "socket://192.168.1.100:9100"
 *   "192.168.1.100:9100"
 *   "192.168.1.100"           (puerto default 9100)
 *   "printer.local:9100"
 *
 * También acepta query strings: "socket://host:9100?contimeout=5000"
 * y rutas con slash final: "tcp://host:9100/"
 */
export function parseTcpEndpoint(printerName: string): TcpEndpoint | null {
    if (!printerName || !printerName.trim()) return null;

    let raw = printerName.trim().replace(/^[/\r\n ]+|[/\r\n ]+$/g, "");

    // Quitar prefijos de protocolo
    if (raw.toLowerCase().startsWith("tcp://")) {
        raw = raw.slice(6);
    } else if (raw.toLowerCase().startsWith("socket://")) {
        raw = raw.slice(9);
    }

    // Quitar slash final
    raw = raw.replace(/\/$/, "");

    // Quitar query string
    const queryIdx = raw.indexOf("?");
    if (queryIdx >= 0) {
        raw = raw.slice(0, queryIdx);
    }

    // Separar host:puerto
    const colonIdx = raw.lastIndexOf(":");
    let host: string;
    let port = DEFAULT_PORT;

    if (colonIdx > 0) {
        const portStr = raw.slice(colonIdx + 1);
        const parsedPort = parseInt(portStr, 10);
        if (!isNaN(parsedPort) && parsedPort > 0) {
            host = raw.slice(0, colonIdx).trim();
            port = parsedPort;
        } else {
            host = raw.trim();
        }
    } else {
        host = raw.trim();
    }

    if (!host) return null;

    // Aceptar IPv4 numérica o hostname con punto sin espacios ni slashes
    const isValidHost =
        isIPv4(host) || (host.includes(".") && !host.includes(" ") && !host.includes("/"));

    if (!isValidHost) return null;

    return { host, port };
}

/** Envía bytes a la impresora TCP. Port de TcpRawPrinter.SendAsync del C#. */
export function sendTcp(host: string, port: number, data: Buffer): Promise<void> {
    return new Promise((resolve, reject) => {
        const socket = new net.Socket();
        let settled = false;

        const done = (err?: Error): void => {
            if (settled) return;
            settled = true;
            socket.destroy();
            if (err) reject(err);
            else resolve();
        };

        const timer = setTimeout(() => {
            done(new Error(`Timeout conectando a ${host}:${port} (${CONNECT_TIMEOUT_MS}ms)`));
        }, CONNECT_TIMEOUT_MS);

        socket.setNoDelay(true);

        socket.once("error", (err) => {
            clearTimeout(timer);
            done(err);
        });

        socket.connect(port, host, () => {
            socket.write(data, (err) => {
                clearTimeout(timer);
                if (err) {
                    done(err);
                } else {
                    socket.end();
                    // Esperar FIN del otro lado (opcional pero limpio)
                    socket.once("close", () => done());
                    // Timeout de cierre
                    setTimeout(() => done(), 1000);
                }
            });
        });
    });
}

function isIPv4(str: string): boolean {
    const parts = str.split(".");
    if (parts.length !== 4) return false;
    return parts.every((p) => {
        const n = parseInt(p, 10);
        return !isNaN(n) && n >= 0 && n <= 255 && String(n) === p;
    });
}
