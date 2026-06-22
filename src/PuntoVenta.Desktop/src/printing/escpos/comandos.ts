/**
 * Constantes y builders de bytes ESC/POS puros.
 * Port fiel de EscPosBuilder.cs. Sin dependencias de Electron.
 *
 * Referencia: https://reference.epson-biz.com/modules/ref_escpos/
 */

// ---------------------------------------------------------------------------
// Constantes — bytes exactos del C#
// ---------------------------------------------------------------------------

/** ESC @ — inicializa la impresora */
export const INIT_PRINTER = Buffer.from([0x1b, 0x40]);

/** ESC a 0 — alineación izquierda */
export const ALIGN_LEFT = Buffer.from([0x1b, 0x61, 0x00]);

/** ESC a 1 — alineación centrada */
export const ALIGN_CENTER = Buffer.from([0x1b, 0x61, 0x01]);

/** ESC a 2 — alineación derecha */
export const ALIGN_RIGHT = Buffer.from([0x1b, 0x61, 0x02]);

/** ESC E 1 — negrita ON */
export const BOLD_ON = Buffer.from([0x1b, 0x45, 0x01]);

/** ESC E 0 — negrita OFF */
export const BOLD_OFF = Buffer.from([0x1b, 0x45, 0x00]);

/** 0x0A — salto de línea */
export const LINE_FEED = Buffer.from([0x0a]);

/**
 * ESC 3 0x3C — interlineado n/180 inch.
 * 0x3C (60) da bastante aire; defaults ZK/Goojprt rondan 30-40.
 */
export const SET_LINE_SPACING = Buffer.from([0x1b, 0x33, 0x3c]);

/** ESC d 3 — avanzar 3 líneas (espacio antes de corte) */
export const FEED_THREE_LINES = Buffer.from([0x1b, 0x64, 0x03]);

/** GS V 66 0 — corte parcial */
export const PARTIAL_CUT = Buffer.from([0x1d, 0x56, 0x42, 0x00]);

/** GS V 65 0 — corte completo */
export const FULL_CUT = Buffer.from([0x1d, 0x56, 0x41, 0x00]);

/**
 * ESC p 0 50 250 — pulso gaveta pin 2 (m=0).
 * t1=50ms on, t2=250ms off — mismos valores que el C#.
 */
export const DRAWER_KICK_PIN0 = Buffer.from([0x1b, 0x70, 0x00, 0x32, 0xfa]);

/**
 * ESC p 1 50 250 — pulso gaveta pin 5 (m=1).
 */
export const DRAWER_KICK_PIN1 = Buffer.from([0x1b, 0x70, 0x01, 0x32, 0xfa]);

// ---------------------------------------------------------------------------
// Tipo del enum ComandoCorte — acepta string (backend JSON) o number
// ---------------------------------------------------------------------------

export type ComandoCorte = "None" | "PartialCut" | "FullCut" | 0 | 1 | 2;

// ---------------------------------------------------------------------------
// Builders
// ---------------------------------------------------------------------------

/**
 * Selecciona tabla de caracteres en la impresora.
 * ESC t n — port directo de WriteCodepageSelector en el C#.
 *
 * CP437 → 0, CP850 → 2, CP858 → 19, CP1252 (Windows-1252) → 16.
 */
export function buildCodepageSelector(codepage: string): Buffer {
    const n = codepageToEscPosTable(codepage);
    return Buffer.from([0x1b, 0x74, n]);
}

export function codepageToEscPosTable(codepage: string): number {
    switch (codepage.toUpperCase()) {
        case "CP437":
            return 0;
        case "CP850":
            return 2;
        case "CP858":
            return 19;
        case "CP1252":
        case "WINDOWS-1252":
            return 16;
        default:
            return 0; // fallback CP437
    }
}

/**
 * Pulso gaveta — solo ESC p m t1 t2 (5 bytes).
 * Port de BuildDrawerKick / WriteDrawerKick del C#.
 * drawerPin === 1 → pin5; cualquier otro → pin2.
 */
export function buildDrawerKick(drawerPin: number): Buffer {
    return drawerPin === 1 ? Buffer.from(DRAWER_KICK_PIN1) : Buffer.from(DRAWER_KICK_PIN0);
}

/**
 * Bytes de corte según el perfil.
 * None → Buffer vacío; PartialCut → GS V B 0; FullCut → GS V A 0.
 */
export function buildCut(comandoCorte: ComandoCorte): Buffer {
    // Normalizar: acepta string o número (robustez ante serialización JSON del backend)
    const normalizado = normalizarComandoCorte(comandoCorte);
    switch (normalizado) {
        case "PartialCut":
            return Buffer.from(PARTIAL_CUT);
        case "FullCut":
            return Buffer.from(FULL_CUT);
        case "None":
        default:
            return Buffer.alloc(0);
    }
}

export function normalizarComandoCorte(c: ComandoCorte): "None" | "PartialCut" | "FullCut" {
    if (c === "None" || c === 0) return "None";
    if (c === "PartialCut" || c === 1) return "PartialCut";
    if (c === "FullCut" || c === 2) return "FullCut";
    return "None";
}

/**
 * Bytes de alineación desde string ("Izquierda" | "Centro" | "Derecha").
 */
export function buildAlineacion(alineacion: string): Buffer {
    switch (alineacion) {
        case "Centro":
            return Buffer.from(ALIGN_CENTER);
        case "Derecha":
            return Buffer.from(ALIGN_RIGHT);
        default:
            return Buffer.from(ALIGN_LEFT);
    }
}

/**
 * Code128 nativo de impresora (GS k 73 n + payload ASCII).
 * Port de WriteBarcodeCode128 del C#.
 * - GS H 0: sin HRI (el consecutivo ya va en el encabezado)
 * - GS h 80: altura de barras
 * - GS w 2: ancho de módulo
 * - GS k 73 n + "{B" + datos
 */
export function buildBarcodeCode128(consecutivo: string): Buffer {
    // Filtrar solo caracteres ASCII imprimibles [0x20, 0x7F)
    const datos = consecutivo
        .split("")
        .filter((c) => c.charCodeAt(0) >= 0x20 && c.charCodeAt(0) < 0x7f)
        .join("");
    if (!datos) return Buffer.alloc(0);

    const payload = Buffer.from("{B" + datos, "ascii");
    return Buffer.concat([
        Buffer.from([0x1d, 0x48, 0x00]), // GS H 0: sin HRI
        Buffer.from([0x1d, 0x68, 0x50]), // GS h 80: altura
        Buffer.from([0x1d, 0x77, 0x02]), // GS w 2: ancho módulo
        Buffer.from([0x1d, 0x6b, 0x49, payload.length]), // GS k 73 n
        payload,
        Buffer.from([0x0a]), // LF
    ]);
}
