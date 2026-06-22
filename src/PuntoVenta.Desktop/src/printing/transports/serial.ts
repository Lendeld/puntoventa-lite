/**
 * Transport serial — impresoras designadas como "serial://<path>?baud=9600".
 * Usa el módulo nativo `serialport`.
 * Import dinámico con error claro si el módulo nativo no carga.
 */

const SERIAL_PREFIX = "serial://";
const DEFAULT_BAUD = 9600;

export interface SerialEndpoint {
    path: string;
    baud: number;
}

/**
 * Parsea "serial:///dev/ttyUSB0?baud=115200" → { path, baud }.
 * Retorna null si el nombre no empieza con "serial://".
 */
export function parseSerialEndpoint(printerName: string): SerialEndpoint | null {
    if (!printerName.toLowerCase().startsWith(SERIAL_PREFIX)) return null;

    const rest = printerName.slice(SERIAL_PREFIX.length);
    const qIdx = rest.indexOf("?");
    const rawPath = qIdx >= 0 ? rest.slice(0, qIdx) : rest;
    const query = qIdx >= 0 ? rest.slice(qIdx + 1) : "";

    let baud = DEFAULT_BAUD;
    for (const part of query.split("&")) {
        const [k, v] = part.split("=");
        if (k?.toLowerCase() === "baud" && v) {
            const parsed = parseInt(v, 10);
            if (!isNaN(parsed) && parsed > 0) baud = parsed;
        }
    }

    const devicePath = rawPath || "";
    if (!devicePath) return null;
    return { path: devicePath, baud };
}

/**
 * Envía bytes a una impresora serial.
 * Import dinámico de `serialport` — si el módulo nativo no está disponible,
 * lanza un error descriptivo en español.
 */
export async function sendSerial(path: string, baud: number, data: Buffer): Promise<void> {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    let SerialPortModule: any;
    try {
        // Dynamic import — serialport es opcional (nativo, puede no compilar en todos los entornos)
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        SerialPortModule = await (eval('import("serialport")') as Promise<unknown>);
    } catch (e) {
        throw new Error(
            `El módulo nativo 'serialport' no está disponible. ` +
            `Instala las dependencias nativas o usa TCP en lugar de serial. ` +
            `Detalle: ${(e as Error).message}`,
        );
    }

    const SerialPort = SerialPortModule.SerialPort ?? SerialPortModule.default?.SerialPort ?? SerialPortModule.default;
    if (!SerialPort) {
        throw new Error("No se pudo obtener la clase SerialPort del módulo. Verifica la versión instalada.");
    }

    return new Promise<void>((resolve, reject) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const port = new SerialPort({ path, baudRate: baud }, (err: any) => {
            if (err) {
                reject(new Error(`No se pudo abrir el puerto serial ${path}: ${err.message}`));
                return;
            }

            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            port.write(data, (writeErr: any) => {
                if (writeErr) {
                    port.close();
                    reject(new Error(`Error escribiendo al puerto serial: ${writeErr.message}`));
                    return;
                }
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                port.drain((drainErr: any) => {
                    port.close();
                    if (drainErr) {
                        reject(new Error(`Error en drain del puerto serial: ${drainErr.message}`));
                    } else {
                        resolve();
                    }
                });
            });
        });

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        port.on("error", (err: any) => {
            reject(new Error(`Error en puerto serial ${path}: ${err.message}`));
        });
    });
}
