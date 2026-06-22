/**
 * Texto → bytes según codepage del perfil.
 * Usa iconv-lite para codificaciones de impresoras térmicas.
 *
 * Codepages soportados: CP437, CP850, CP858, CP1252 (Windows-1252).
 * Port de ResolveEncoding / WriteCodepageSelector del C#.
 */

import iconvLite from "iconv-lite";

// ---------------------------------------------------------------------------
// Mapa nombre-canonico → charset de iconv-lite
// ---------------------------------------------------------------------------
const CODEPAGE_MAP: Record<string, string> = {
    CP437: "CP437",
    CP850: "CP850",
    CP858: "CP858",
    CP1252: "CP1252",
    "WINDOWS-1252": "CP1252",
};

/**
 * Devuelve el charset de iconv-lite para un codepage de perfil.
 * Fallback a CP437 (compatible con la mayoría de impresoras ZK/Epson).
 */
export function resolveCharset(codepage: string): string {
    return CODEPAGE_MAP[codepage.toUpperCase()] ?? "CP437";
}

/**
 * Codifica un string al Buffer de bytes del codepage indicado.
 * Iconv-lite reemplaza caracteres no mapeables con '?'.
 * Tildes y ñ se codifican correctamente en CP850/CP858/CP1252.
 */
export function encodeText(text: string, codepage: string): Buffer {
    const charset = resolveCharset(codepage);
    return iconvLite.encode(text, charset);
}

/**
 * Sanitiza el texto igual que el C#: filtra bytes de control
 * (< 0x20) excepto LF, CR, TAB para evitar inyección de comandos ESC/POS.
 */
export function sanitize(input: string): string {
    if (!input) return "";
    let result = "";
    for (const ch of input) {
        const code = ch.charCodeAt(0);
        if (code >= 0x20 || code === 0x0a || code === 0x0d || code === 0x09) {
            result += ch;
        }
    }
    return result;
}
