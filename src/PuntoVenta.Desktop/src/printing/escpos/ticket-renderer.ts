/**
 * Port fiel del layout de EscPosBuilder.BuildTicket (C#).
 * TicketData + PerfilImpresoraTicket → Buffer listo para enviar a la impresora.
 *
 * Decisiones del port:
 * - Logo (LogoUrl/MostrarLogo): TODO — omitido en v1 (decode de imágenes en ESC/POS
 *   requiere rasterización; complejo y no usada en el flujo actual).
 * - Barcode Code128: portado vía buildBarcodeCode128 (GS k 73, nativo de impresora).
 * - Densidad: perfil lleva el campo pero ESC/POS no tiene comando estandarizado de
 *   densidad de impresión universal; el C# tampoco lo aplica → omitido.
 * - ReferenciaCodigoRazon: eliminado en F2; la rama de código que lo usa en el C#
 *   se dejó como código muerto (if false → nunca ejecuta).
 */

import {
    INIT_PRINTER,
    ALIGN_LEFT,
    ALIGN_CENTER,
    BOLD_ON,
    BOLD_OFF,
    LINE_FEED,
    SET_LINE_SPACING,
    FEED_THREE_LINES,
    buildCodepageSelector,
    buildDrawerKick,
    buildCut,
    buildAlineacion,
    buildBarcodeCode128,
    ComandoCorte,
} from "./comandos";
import { encodeText, sanitize } from "./encoder";

// ---------------------------------------------------------------------------
// Tipos — mirror de TicketDataDto (camelCase) del frontend
// ---------------------------------------------------------------------------

export interface TicketEncabezadoLinea {
    texto: string;
    negrita: boolean;
}

export interface TicketLinea {
    codigo: string;
    descripcion: string;
    cantidad: number;
    unidadMedidaCodigo: string;
    precioUnitario: number;
    descuento: number;
    porcentajeImpuesto: number;
    total: number;
}

export interface TicketPago {
    id: string;
    fechaUtc: string;
    medioPagoDetalle: string;
    monedaCodigo: string;
    montoAplicado: number;
    montoEntregado: number;
    montoVuelto: number;
    referencia: string | null;
    numeroAbono: number;
    fechaRegistroUtc: string | null;
    anulado: boolean;
    fechaAnulacionUtc: string | null;
    usuarioAnulaNombre: string | null;
    motivoAnulacion: string | null;
}

export interface TicketLineaPie {
    texto: string;
    alineacion: "Izquierda" | "Centro" | "Derecha";
    negrita: boolean;
}

export interface TicketData {
    encabezado: string;
    direccion: string | null;
    identificacionFiscal: string | null;
    telefono: string | null;
    correo: string | null;
    logoUrl: string | null;
    mostrarLogo: boolean;
    tipoDocumento: string;
    consecutivo: string;
    fechaUtc: string; // ISO UTC string
    cajaCodigo: string | null;
    cajaNombre: string | null;
    vendedorNombre: string | null;
    condicionVentaDetalle: string;
    clienteNombre: string;
    clienteIdentificacion: string | null;
    lineas: TicketLinea[];
    pagos: TicketPago[];
    subtotal: number;
    descuentos: number;
    impuestos: number;
    total: number;
    pagado: number;
    saldo: number;
    monedaCodigo: string;
    tipoCambio: number;
    mensajePie: string | null;
    observaciones: string | null;
    aplicaCopiaClienteNegocio: boolean;
    mostrarCodigoBarras: boolean;
    lineasPie: TicketLineaPie[];
    referenciaTipoDocumento: string | null;
    referenciaConsecutivo: string | null;
    referenciaRazon: string | null;
    lineasEncabezado: TicketEncabezadoLinea[] | null;
    esRecibo: boolean;
    saldoAnterior: number;
    saldoNuevo: number;
    esReciboAnulado: boolean;
    fechaAnulacionUtc: string | null;
    usuarioAnulaNombre: string | null;
    motivoAnulacion: string | null;
}

export interface PerfilImpresoraTicket {
    clave: string;
    nombre: string;
    anchoMm: number;
    charsPorLinea: number;
    codepage: string;
    drawerPin: number;
    comandoCorte: ComandoCorte;
    densidad: number;
}

// ---------------------------------------------------------------------------
// Helpers internos
// ---------------------------------------------------------------------------

/** Convierte UTC ISO string a hora de Costa Rica (GMT-6, sin DST) formateada */
function formatFechaCR(fechaUtcStr: string): string {
    const utc = new Date(fechaUtcStr);
    // Costa Rica: UTC-6, sin DST
    const crOffsetMs = -6 * 60 * 60 * 1000;
    const local = new Date(utc.getTime() + crOffsetMs);

    const dd = String(local.getUTCDate()).padStart(2, "0");
    const mm = String(local.getUTCMonth() + 1).padStart(2, "0");
    const yyyy = local.getUTCFullYear();
    let hh = local.getUTCHours();
    const min = String(local.getUTCMinutes()).padStart(2, "0");
    const ampm = hh >= 12 ? "PM" : "AM";
    hh = hh % 12 || 12;
    return `${dd}/${mm}/${yyyy} ${String(hh).padStart(2, "0")}:${min} ${ampm}`;
}

/** Formatea decimal con 2 decimales y separadores de miles */
function fmtDecimal(value: number): string {
    return value.toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
        useGrouping: true,
    });
}

/**
 * Devuelve el codepage efectivo: si el perfil es ZKTeco, fuerza CP437.
 * Port de EffectiveCodepage + IsZkteco del C#.
 */
function effectiveCodepage(profile: PerfilImpresoraTicket): string {
    const isZkteco =
        profile.clave.toLowerCase().includes("zkteco") ||
        profile.nombre.toLowerCase().includes("zkteco");
    return isZkteco ? "CP437" : profile.codepage;
}

// ---------------------------------------------------------------------------
// Escritores de secciones (internos)
// ---------------------------------------------------------------------------

class TicketWriter {
    private parts: Buffer[] = [];
    private readonly width: number;
    private readonly codepage: string;

    constructor(width: number, codepage: string) {
        this.width = width;
        this.codepage = codepage;
    }

    write(buf: Buffer | Uint8Array): void {
        this.parts.push(Buffer.from(buf));
    }

    writeLine(text: string): void {
        this.write(encodeText(sanitize(text), this.codepage));
        this.write(LINE_FEED);
    }

    /** Escribe texto envuelto por palabras al ancho de la impresora */
    writeWrapped(text: string): void {
        for (const line of wrapWords(text, this.width)) {
            this.write(encodeText(line, this.codepage));
            this.write(LINE_FEED);
        }
    }

    /** Dos columnas: izquierda + relleno de espacios + derecha */
    writeTwoCol(left: string, right: string): void {
        let l = sanitize(left);
        const r = sanitize(right);
        if (l.length + r.length + 1 > this.width) {
            l = l.substring(0, Math.max(0, this.width - r.length - 1));
        }
        let space = this.width - l.length - r.length;
        if (space < 1) space = 1;
        const line = l + " ".repeat(space) + r;
        this.write(encodeText(line, this.codepage));
        this.write(LINE_FEED);
    }

    writeSeparator(): void {
        const line = "-".repeat(this.width);
        // Separador en ASCII puro (igual que el C# — siempre ASCII.GetBytes)
        this.write(Buffer.from(line, "ascii"));
        this.write(LINE_FEED);
    }

    /** Banda de guiones con la etiqueta centrada: "---- CLIENTE ----" */
    writeLabelBand(label: string): void {
        const texto = ` ${sanitize(label).trim().toUpperCase()} `;
        let line: string;
        if (texto.length >= this.width) {
            line = texto.trim();
        } else {
            const izq = Math.floor((this.width - texto.length) / 2);
            const der = this.width - texto.length - izq;
            line = "-".repeat(izq) + texto + "-".repeat(der);
        }
        this.write(encodeText(line, this.codepage));
        this.write(LINE_FEED);
    }

    toBuffer(): Buffer {
        return Buffer.concat(this.parts);
    }
}

/** Port de WrapWords del C# */
function wrapWords(text: string, width: number): string[] {
    const clean = sanitize(text).trim();
    if (!clean) return [""];
    if (width < 1) return [clean];

    const lines: string[] = [];
    let current = "";

    for (const rawWord of clean.split(/\s+/).filter(Boolean)) {
        let word = rawWord;

        // Palabra más larga que el ancho → trocear duro
        while (word.length > width) {
            if (current.length > 0) {
                lines.push(current);
                current = "";
            }
            lines.push(word.substring(0, width));
            word = word.substring(width);
        }

        if (!word) continue;

        if (current.length === 0) {
            current = word;
        } else if (current.length + 1 + word.length <= width) {
            current += " " + word;
        } else {
            lines.push(current);
            current = word;
        }
    }

    if (current.length > 0) lines.push(current);
    return lines;
}

function writeReciboSection(w: TicketWriter, data: TicketData): void {
    const pago = data.pagos[0];
    const montoMovimiento = Math.abs(pago?.montoAplicado ?? data.total);

    w.write(BOLD_ON);
    w.writeWrapped(data.esReciboAnulado ? "ANULACION DE ABONO" : "RECIBO DE ABONO");
    w.write(BOLD_OFF);
    w.writeSeparator();

    if (pago?.numeroAbono && pago.numeroAbono > 0) {
        w.writeWrapped(`Abono #${pago.numeroAbono}`);
    }

    w.writeWrapped(
        `Fecha informativa: ${formatFechaCR(pago?.fechaUtc ?? data.fechaUtc)}`,
    );
    w.writeWrapped(
        `Registro real: ${formatFechaCR(pago?.fechaRegistroUtc ?? data.fechaUtc)}`,
    );
    if (pago) {
        w.writeWrapped(`Medio: ${pago.medioPagoDetalle}`);
    }
    if (pago?.referencia) {
        w.writeWrapped(`Referencia: ${pago.referencia}`);
    }

    w.writeSeparator();
    w.write(BOLD_ON);
    w.writeTwoCol(
        data.esReciboAnulado ? "Monto revertido" : "Monto abonado",
        fmtDecimal(montoMovimiento),
    );
    w.write(BOLD_OFF);
    w.writeTwoCol(
        data.esReciboAnulado ? "Saldo antes de anular" : "Saldo anterior",
        fmtDecimal(data.saldoAnterior),
    );
    w.writeTwoCol("Saldo pendiente", fmtDecimal(data.saldoNuevo));

    if (data.esReciboAnulado) {
        w.writeSeparator();
        if (data.fechaAnulacionUtc) {
            w.writeWrapped(`Fecha anulacion: ${formatFechaCR(data.fechaAnulacionUtc)}`);
        }
        if (data.usuarioAnulaNombre) {
            w.writeWrapped(`Anulado por: ${data.usuarioAnulaNombre}`);
        }
        if (data.motivoAnulacion) {
            w.writeWrapped(`Motivo: ${data.motivoAnulacion}`);
        }
    } else if (data.observaciones) {
        w.writeSeparator();
        w.writeWrapped(data.observaciones);
    }
}

// ---------------------------------------------------------------------------
// BuildTicket — port de EscPosBuilder.BuildTicket
// ---------------------------------------------------------------------------

export function buildTicket(
    data: TicketData,
    profile: PerfilImpresoraTicket,
    drawer: boolean,
    copiaLabel?: string,
): Buffer {
    const codepage = effectiveCodepage(profile);
    const w = new TicketWriter(profile.charsPorLinea, codepage);

    // Init
    w.write(INIT_PRINTER);
    w.write(SET_LINE_SPACING);
    w.write(buildCodepageSelector(codepage));

    // -- Encabezado --
    w.write(ALIGN_CENTER);
    if (data.lineasEncabezado && data.lineasEncabezado.length > 0) {
        for (const linea of data.lineasEncabezado) {
            if (linea.negrita) w.write(BOLD_ON);
            w.writeWrapped(linea.texto);
            if (linea.negrita) w.write(BOLD_OFF);
        }
    } else {
        // Encabezado fijo histórico
        w.write(BOLD_ON);
        w.writeWrapped(data.encabezado);
        w.write(BOLD_OFF);

        if (data.identificacionFiscal) {
            w.writeWrapped(`Cedula: ${data.identificacionFiscal}`);
        }
        if (data.telefono) {
            w.writeWrapped(`Tel: ${data.telefono}`);
        }
        if (data.correo) {
            w.writeWrapped(data.correo);
        }
        if (data.direccion) {
            w.writeWrapped(data.direccion);
        }
        // Fecha en encabezado (centrada)
        w.writeWrapped(formatFechaCR(data.fechaUtc));
    }

    w.writeSeparator();

    // Tipo de documento (negrita, centrado) entre separadores
    w.write(BOLD_ON);
    w.writeWrapped(data.tipoDocumento);
    w.write(BOLD_OFF);

    w.writeSeparator();

    // -- Datos del documento --
    w.write(ALIGN_LEFT);
    const consecutivo = data.consecutivo || "-";
    w.writeWrapped(`Consecutivo: ${consecutivo}`);

    if (data.referenciaConsecutivo) {
        const etiqueta = data.referenciaTipoDocumento || "Ref";
        w.writeLine(`Ref: ${etiqueta} ${data.referenciaConsecutivo}`);
        // ReferenciaCodigoRazon eliminado en F2 — solo ReferenciaRazon
        if (data.referenciaRazon) {
            w.writeLine(`Razon: ${data.referenciaRazon}`);
        }
    }

    w.writeWrapped(`Cliente: ${data.clienteNombre}`);
    if (data.clienteIdentificacion) {
        w.writeWrapped(`Id: ${data.clienteIdentificacion}`);
    }

    // Separador entre cliente y metadata de caja
    w.writeSeparator();

    if (data.cajaCodigo) {
        const caja = data.cajaNombre
            ? `${data.cajaCodigo} - ${data.cajaNombre}`
            : data.cajaCodigo;
        w.writeWrapped(`Caja: ${caja}`);
    }
    if (data.vendedorNombre) {
        w.writeWrapped(`Vendedor: ${data.vendedorNombre}`);
    }
    if (data.condicionVentaDetalle) {
        w.writeLine(`Condicion: ${data.condicionVentaDetalle}`);
    }
    const monedaLinea =
        data.tipoCambio !== 1
            ? `Moneda: ${data.monedaCodigo}  TC: ${fmtDecimal(data.tipoCambio)}`
            : `Moneda: ${data.monedaCodigo}`;
    w.writeLine(monedaLinea);

    w.writeSeparator();

    if (data.esRecibo) {
        writeReciboSection(w, data);

        // -- MensajePie --
        if (data.mensajePie) {
            w.write(LINE_FEED);
            w.write(ALIGN_CENTER);
            w.writeWrapped(data.mensajePie);
            w.write(ALIGN_LEFT);
        }

        // -- LineasPie --
        if (data.lineasPie.length > 0) {
            w.write(LINE_FEED);
            for (const linea of data.lineasPie) {
                w.write(buildAlineacion(linea.alineacion));
                if (linea.negrita) w.write(BOLD_ON);
                w.writeWrapped(linea.texto);
                if (linea.negrita) w.write(BOLD_OFF);
            }
            w.write(ALIGN_LEFT);
        }

        w.write(FEED_THREE_LINES);

        if (drawer) {
            w.write(buildDrawerKick(profile.drawerPin));
        }

        w.write(buildCut(profile.comandoCorte));
        return w.toBuffer();
    }

    // -- Líneas de productos --
    for (const linea of data.lineas) {
        const tituloLinea = linea.codigo
            ? `${linea.descripcion} - ${linea.codigo}`
            : linea.descripcion;
        w.writeWrapped(tituloLinea);
        const detalle = `  ${fmtDecimal(linea.cantidad)} x ${fmtDecimal(linea.precioUnitario)} IVA ${fmtPct(linea.porcentajeImpuesto)}%`;
        w.writeTwoCol(detalle, fmtDecimal(linea.total));
    }

    w.writeSeparator();

    // -- Totales --
    w.writeTwoCol("Subtotal", fmtDecimal(data.subtotal));
    w.writeTwoCol("Descuentos", fmtDecimal(-data.descuentos));

    // Impuestos desglosados por porcentaje (port del C#)
    interface ImpGrupo {
        pct: number;
        monto: number;
    }
    const impGrupos = new Map<number, number>();
    for (const l of data.lineas) {
        const neto = Math.max(0, l.cantidad * l.precioUnitario - l.descuento);
        const monto = Math.round((neto * (l.porcentajeImpuesto / 100)) * 100) / 100;
        impGrupos.set(l.porcentajeImpuesto, (impGrupos.get(l.porcentajeImpuesto) ?? 0) + monto);
    }
    const impList: ImpGrupo[] = Array.from(impGrupos.entries())
        .filter(([, monto]) => monto !== 0)
        .map(([pct, monto]) => ({ pct, monto }))
        .sort((a, b) => a.pct - b.pct);

    for (const { pct, monto } of impList) {
        w.writeTwoCol(`Impuesto ${fmtPct(pct)}%`, fmtDecimal(monto));
    }

    w.write(BOLD_ON);
    w.writeTwoCol(`TOTAL ${data.monedaCodigo}`, fmtDecimal(data.total));
    w.write(BOLD_OFF);

    // Equivalente en la otra moneda al TC del documento
    if (data.tipoCambio > 0 && data.tipoCambio !== 1) {
        const esUsd = data.monedaCodigo.toUpperCase() === "USD";
        const monedaEquivalente = esUsd ? "CRC" : "USD";
        const totalEquivalente = esUsd
            ? data.total * data.tipoCambio
            : data.total / data.tipoCambio;
        w.writeTwoCol(`Equiv. ${monedaEquivalente}`, fmtDecimal(totalEquivalente));
    }

    // -- Pagos --
    if (data.pagos.length > 0) {
        w.writeSeparator();
        for (const pago of data.pagos) {
            w.writeTwoCol(pago.medioPagoDetalle, fmtDecimal(pago.montoEntregado));
            w.writeTwoCol("  Cambio", fmtDecimal(pago.montoVuelto));
        }
        if (data.saldo !== 0) {
            w.writeTwoCol("Pagado", fmtDecimal(data.pagado));
            w.writeTwoCol("Saldo", fmtDecimal(data.saldo));
        }
    }

    // -- Observaciones --
    if (data.observaciones) {
        w.writeSeparator();
        w.writeWrapped(data.observaciones);
    }

    // -- MensajePie --
    if (data.mensajePie) {
        w.write(LINE_FEED);
        w.write(ALIGN_CENTER);
        w.writeWrapped(data.mensajePie);
        w.write(ALIGN_LEFT);
    }

    // -- LineasPie --
    if (data.lineasPie.length > 0) {
        w.write(LINE_FEED);
        for (const linea of data.lineasPie) {
            w.write(buildAlineacion(linea.alineacion));
            if (linea.negrita) w.write(BOLD_ON);
            w.writeWrapped(linea.texto);
            if (linea.negrita) w.write(BOLD_OFF);
        }
        w.write(ALIGN_LEFT);
    }

    // Resolución MH (pie legal fijo)
    w.write(LINE_FEED);
    w.write(ALIGN_CENTER);
    w.writeLine("MH-DGT-RES-0027-2024");
    w.writeLine("Resolucion General sobre las");
    w.writeLine("disposiciones tecnicas de los");
    w.writeLine("comprobantes electronicos");
    w.write(ALIGN_LEFT);

    // -- Código de barras Code128 (nativo de impresora) --
    if (data.mostrarCodigoBarras && data.consecutivo) {
        w.write(LINE_FEED);
        w.write(ALIGN_CENTER);
        const barcode = buildBarcodeCode128(data.consecutivo);
        if (barcode.length > 0) {
            w.write(barcode);
        }
        w.write(ALIGN_LEFT);
    }

    // -- Logo: TODO — omitido en v1 --
    // TODO: implementar logo cuando se requiera (decode PNG/JPEG → rasterización
    // ESC/POS con GS v 0 o GS ( L). Requiere biblioteca de imágenes externa
    // y validación de ancho en pixels según AnchoMm.

    // -- Rótulo de copia (Cliente / Negocio) --
    if (copiaLabel) {
        w.write(LINE_FEED);
        w.write(ALIGN_CENTER);
        w.writeLabelBand(copiaLabel);
        w.write(ALIGN_LEFT);
    }

    // Espacio antes del corte
    w.write(FEED_THREE_LINES);

    // Gaveta ANTES del corte (igual que el C#: WriteDrawerKick antes de WriteCut)
    if (drawer) {
        w.write(buildDrawerKick(profile.drawerPin));
    }

    // Corte
    w.write(buildCut(profile.comandoCorte));

    return w.toBuffer();
}

/**
 * Ticket de prueba — port de BuildTestPage del C#.
 * Incluye info del perfil, alineaciones y texto con acentos.
 */
export function buildTestPage(profile: PerfilImpresoraTicket): Buffer {
    const codepage = effectiveCodepage(profile);
    const w = new TicketWriter(profile.charsPorLinea, codepage);

    w.write(INIT_PRINTER);
    w.write(buildCodepageSelector(codepage));
    w.write(ALIGN_CENTER);
    w.write(BOLD_ON);
    w.writeLine("PRUEBA DE IMPRESION");
    w.write(BOLD_OFF);
    w.write(ALIGN_LEFT);
    w.writeLine(`Perfil: ${profile.nombre}`);
    w.writeLine(`Clave:  ${profile.clave}`);
    w.writeLine(`Ancho:  ${profile.anchoMm}mm / ${profile.charsPorLinea} chars`);
    w.writeLine(`Codepage: ${codepage}`);
    w.writeSeparator();
    w.writeLine("Texto simple ASCII OK");
    w.writeLine("Numeros: 0123456789");
    // Test de acentos/ñ — esto verifica que el codepage resuelve bien
    w.writeLine("Acentos: cafe, muneca, jabon");
    w.writeLine("Mayus: ACCION, SENOR, ESPANOL");
    w.writeSeparator();
    // Fecha actual en formato CR
    w.writeLine(formatFechaCR(new Date().toISOString()));
    w.write(FEED_THREE_LINES);
    w.write(buildCut(profile.comandoCorte));

    return w.toBuffer();
}

// ---------------------------------------------------------------------------
// Solo gaveta
// ---------------------------------------------------------------------------
export function buildDrawerOnly(profile: PerfilImpresoraTicket): Buffer {
    return buildDrawerKick(profile.drawerPin);
}

// ---------------------------------------------------------------------------
// helpers privados
// ---------------------------------------------------------------------------
function fmtPct(pct: number): string {
    // Eliminar ceros innecesarios: 13.00 → "13", 1.50 → "1.5"
    return pct % 1 === 0 ? String(pct) : String(parseFloat(pct.toFixed(2)));
}
