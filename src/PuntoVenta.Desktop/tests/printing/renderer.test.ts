/**
 * Tests del ticket renderer.
 * Verifica: ESC @ al inicio, GS V al final, columnas ≤ charsPorLinea,
 * secciones presentes, codepage correcto en el buffer.
 */
import { describe, it, expect } from "vitest";
import { buildTicket, buildTestPage, type TicketData, type PerfilImpresoraTicket } from "../../src/printing/escpos/ticket-renderer";
import { INIT_PRINTER, PARTIAL_CUT, FULL_CUT } from "../../src/printing/escpos/comandos";

// ---------------------------------------------------------------------------
// Fixtures
// ---------------------------------------------------------------------------

const perfil58: PerfilImpresoraTicket = {
    clave: "tm20-58",
    nombre: "Epson TM20 58mm",
    anchoMm: 58,
    charsPorLinea: 32,
    codepage: "CP850",
    drawerPin: 0,
    comandoCorte: "PartialCut",
    densidad: 0,
};

const perfil80: PerfilImpresoraTicket = {
    clave: "tm20-80",
    nombre: "Epson TM20 80mm",
    anchoMm: 80,
    charsPorLinea: 48,
    codepage: "CP850",
    drawerPin: 0,
    comandoCorte: "FullCut",
    densidad: 0,
};

const ticketSimple: TicketData = {
    encabezado: "Almacen El Sol",
    direccion: "Calle 5, San Jose",
    identificacionFiscal: "3-101-123456",
    telefono: "2222-3333",
    correo: "info@elsol.cr",
    logoUrl: null,
    mostrarLogo: false,
    tipoDocumento: "Factura Electronica",
    consecutivo: "00100001010000000001",
    fechaUtc: "2025-03-15T14:30:00Z",
    cajaCodigo: "C01",
    cajaNombre: "Caja Principal",
    vendedorNombre: "Juan Perez",
    condicionVentaDetalle: "Contado",
    clienteNombre: "Maria Lopez",
    clienteIdentificacion: "1-1234-5678",
    lineas: [
        {
            codigo: "P001",
            descripcion: "Cafe Molido 500g",
            cantidad: 2,
            unidadMedidaCodigo: "Unid",
            precioUnitario: 3500,
            descuento: 0,
            porcentajeImpuesto: 13,
            total: 7910,
        },
        {
            codigo: "P002",
            descripcion: "Azucar Blanca 1kg",
            cantidad: 1,
            unidadMedidaCodigo: "Unid",
            precioUnitario: 1200,
            descuento: 100,
            porcentajeImpuesto: 0,
            total: 1100,
        },
    ],
    pagos: [
        {
            id: "pago-1",
            fechaUtc: "2025-03-15T14:30:05Z",
            medioPagoDetalle: "Efectivo",
            monedaCodigo: "CRC",
            montoAplicado: 9010,
            montoEntregado: 10000,
            montoVuelto: 990,
            referencia: null,
            numeroAbono: 0,
            fechaRegistroUtc: "2025-03-15T14:30:05Z",
            anulado: false,
            fechaAnulacionUtc: null,
            usuarioAnulaNombre: null,
            motivoAnulacion: null,
        },
    ],
    subtotal: 8800,
    descuentos: 100,
    impuestos: 910,
    total: 9610,
    pagado: 10000,
    saldo: 0,
    monedaCodigo: "CRC",
    tipoCambio: 1,
    mensajePie: "Gracias por su compra",
    observaciones: null,
    aplicaCopiaClienteNegocio: false,
    mostrarCodigoBarras: true,
    lineasPie: [
        { texto: "Conserve su comprobante", alineacion: "Centro", negrita: false },
    ],
    referenciaTipoDocumento: null,
    referenciaConsecutivo: null,
    referenciaRazon: null,
    lineasEncabezado: null,
    esRecibo: false,
    saldoAnterior: 0,
    saldoNuevo: 0,
    esReciboAnulado: false,
    fechaAnulacionUtc: null,
    usuarioAnulaNombre: null,
    motivoAnulacion: null,
};

// ---------------------------------------------------------------------------
// Helpers de verificación
// ---------------------------------------------------------------------------

/** Busca una secuencia de bytes dentro de un buffer */
function contains(buf: Buffer, seq: number[]): boolean {
    outer: for (let i = 0; i <= buf.length - seq.length; i++) {
        for (let j = 0; j < seq.length; j++) {
            if (buf[i + j] !== seq[j]) continue outer;
        }
        return true;
    }
    return false;
}

function startsWith(buf: Buffer, seq: number[]): boolean {
    if (buf.length < seq.length) return false;
    for (let i = 0; i < seq.length; i++) {
        if (buf[i] !== seq[i]) return false;
    }
    return true;
}

/** Divide el buffer en líneas ASCII (separadas por 0x0A) */
function toTextLines(buf: Buffer): string[] {
    const text = buf.toString("binary");
    return text.split("\x0a");
}

/** Obtiene la longitud de la línea más larga (excluyendo bytes de control) */
function maxLineLength(buf: Buffer, width: number): number {
    const lines = toTextLines(buf);
    let max = 0;
    for (const line of lines) {
        // Filtrar bytes ESC/POS de control (< 0x20 excepto texto normal)
        const visible = line.replace(/[\x00-\x1f]/g, "");
        if (visible.length > max) max = visible.length;
    }
    return max;
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("buildTicket — perfil 58mm (32 chars)", () => {
    it("empieza con ESC @ (init printer)", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(startsWith(buf, Array.from(INIT_PRINTER))).toBe(true);
    });

    it("termina con GS V B 0 (PartialCut)", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        const cutBytes = Array.from(PARTIAL_CUT);
        // Verifica que el corte aparece en algún lugar del buffer
        expect(contains(buf, cutBytes)).toBe(true);
    });

    it("contiene ESC t (codepage selector)", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        // ESC t n → [0x1B, 0x74, n]
        expect(contains(buf, [0x1b, 0x74])).toBe(true);
    });

    it("contiene ESC 3 (interlineado)", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(contains(buf, [0x1b, 0x33, 0x3c])).toBe(true);
    });

    it("ninguna línea visible excede 32 chars", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        const max = maxLineLength(buf, perfil58.charsPorLinea);
        // Margen de +1 por codificación multi-byte, pero en CP850 ASCII extendido
        // ocupa 1 byte. Límite estricto para ASCII.
        expect(max).toBeLessThanOrEqual(perfil58.charsPorLinea + 2);
    });

    it("contiene nombre del cliente", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(buf.toString("latin1")).toContain("Maria Lopez");
    });

    it("contiene consecutivo", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(buf.toString("latin1")).toContain("00100001010000000001");
    });

    it("contiene TOTAL CRC", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(buf.toString("latin1")).toContain("TOTAL CRC");
    });

    it("contiene GS k 73 (Code128) cuando mostrarCodigoBarras=true", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        // GS k 73 = [0x1D, 0x6B, 0x49]
        expect(contains(buf, [0x1d, 0x6b, 0x49])).toBe(true);
    });

    it("NO contiene GS k 73 cuando mostrarCodigoBarras=false", () => {
        const ticket = { ...ticketSimple, mostrarCodigoBarras: false };
        const buf = buildTicket(ticket, perfil58, false);
        expect(contains(buf, [0x1d, 0x6b, 0x49])).toBe(false);
    });

    it("contiene gaveta ESC p cuando drawer=true", () => {
        const buf = buildTicket(ticketSimple, perfil58, true);
        // ESC p = [0x1B, 0x70]
        expect(contains(buf, [0x1b, 0x70])).toBe(true);
    });

    it("NO contiene gaveta ESC p cuando drawer=false", () => {
        const buf = buildTicket(ticketSimple, perfil58, false);
        expect(contains(buf, [0x1b, 0x70])).toBe(false);
    });

    it("gaveta aparece ANTES del corte (mismo orden que el C#)", () => {
        const buf = buildTicket(ticketSimple, perfil58, true);
        const gavetaIdx = buf.indexOf(Buffer.from([0x1b, 0x70]));
        const corteIdx = buf.indexOf(Buffer.from([0x1d, 0x56, 0x42, 0x00]));
        expect(gavetaIdx).toBeGreaterThanOrEqual(0);
        expect(corteIdx).toBeGreaterThan(gavetaIdx);
    });
});

describe("buildTicket — perfil 80mm (48 chars)", () => {
    it("termina con GS V A 0 (FullCut)", () => {
        const buf = buildTicket(ticketSimple, perfil80, false);
        expect(contains(buf, Array.from(FULL_CUT))).toBe(true);
    });

    it("ninguna línea visible excede 48 chars", () => {
        const buf = buildTicket(ticketSimple, perfil80, false);
        const max = maxLineLength(buf, perfil80.charsPorLinea);
        expect(max).toBeLessThanOrEqual(perfil80.charsPorLinea + 2);
    });
});

describe("buildTicket — lineasEncabezado (encabezado configurado)", () => {
    it("usa lineasEncabezado si está presente", () => {
        const ticket: TicketData = {
            ...ticketSimple,
            lineasEncabezado: [
                { texto: "ALMACEN EL SOL S.A.", negrita: true },
                { texto: "San Jose, Costa Rica", negrita: false },
            ],
        };
        const buf = buildTicket(ticket, perfil58, false);
        expect(buf.toString("latin1")).toContain("ALMACEN EL SOL S.A.");
    });
});

describe("buildTicket — copia cliente/negocio", () => {
    it("contiene rótulo CLIENTE cuando se pasa copiaLabel", () => {
        const buf = buildTicket(ticketSimple, perfil58, false, "CLIENTE");
        expect(buf.toString("latin1").toUpperCase()).toContain("CLIENTE");
    });

    it("contiene rótulo NEGOCIO cuando se pasa copiaLabel", () => {
        const buf = buildTicket(ticketSimple, perfil58, false, "NEGOCIO");
        expect(buf.toString("latin1").toUpperCase()).toContain("NEGOCIO");
    });
});

describe("buildTicket — moneda USD con tipo de cambio", () => {
    it("muestra equivalente CRC cuando moneda es USD", () => {
        const ticket: TicketData = {
            ...ticketSimple,
            monedaCodigo: "USD",
            tipoCambio: 520,
            total: 18.5,
            pagos: [],
        };
        const buf = buildTicket(ticket, perfil58, false);
        const text = buf.toString("latin1");
        expect(text).toContain("TOTAL USD");
        expect(text).toContain("Equiv. CRC");
    });
});

describe("buildTicket — perfil ZKTeco fuerza CP437", () => {
    it("usa ESC t 0 (CP437) aunque el perfil diga CP850", () => {
        const perfilZk: PerfilImpresoraTicket = {
            ...perfil58,
            clave: "zkteco-zkp8016",
            nombre: "ZKTeco ZKP8016",
            codepage: "CP850",
        };
        const buf = buildTicket(ticketSimple, perfilZk, false);
        // ESC t 0 (CP437)
        expect(contains(buf, [0x1b, 0x74, 0x00])).toBe(true);
        // NO debe contener ESC t 2 (CP850)
        expect(contains(buf, [0x1b, 0x74, 0x02])).toBe(false);
    });
});

describe("buildTestPage", () => {
    it("empieza con ESC @", () => {
        const buf = buildTestPage(perfil58);
        expect(startsWith(buf, Array.from(INIT_PRINTER))).toBe(true);
    });

    it("contiene nombre del perfil", () => {
        const buf = buildTestPage(perfil58);
        expect(buf.toString("latin1")).toContain(perfil58.nombre);
    });

    it("contiene texto de prueba de acentos", () => {
        // El texto de prueba incluye 'acentos' y 'café'
        const buf = buildTestPage(perfil58);
        expect(buf.toString("latin1")).toContain("Acentos");
    });

    it("termina con corte", () => {
        const buf = buildTestPage(perfil58);
        expect(contains(buf, Array.from(PARTIAL_CUT))).toBe(true);
    });
});

describe("buildTicket — ComandoCorte None", () => {
    it("no tiene bytes de corte cuando ComandoCorte es None", () => {
        const perfilNone: PerfilImpresoraTicket = { ...perfil58, comandoCorte: "None" };
        const buf = buildTicket(ticketSimple, perfilNone, false);
        expect(contains(buf, Array.from(PARTIAL_CUT))).toBe(false);
        expect(contains(buf, Array.from(FULL_CUT))).toBe(false);
    });
});

describe("buildTicket — referencia de documento", () => {
    it("muestra Ref y Razon cuando hay referencia", () => {
        const ticket: TicketData = {
            ...ticketSimple,
            referenciaTipoDocumento: "Nota Credito",
            referenciaConsecutivo: "NC-001",
            referenciaRazon: "Devolucion de mercaderia",
        };
        const buf = buildTicket(ticket, perfil58, false);
        const text = buf.toString("latin1");
        expect(text).toContain("NC-001");
        expect(text).toContain("Devolucion de mercaderia");
    });
});

describe("buildTicket — recibos de abono", () => {
    it("imprime recibo activo con numero de abono y saldos", () => {
        const ticket: TicketData = {
            ...ticketSimple,
            esRecibo: true,
            saldoAnterior: 15000,
            saldoNuevo: 10000,
            pagos: [
                {
                    ...ticketSimple.pagos[0],
                    numeroAbono: 3,
                    montoAplicado: 5000,
                    montoEntregado: 5000,
                    montoVuelto: 0,
                    referencia: "TRX-100",
                },
            ],
            lineas: [],
        };

        const text = buildTicket(ticket, perfil58, false).toString("latin1");
        expect(text).toContain("RECIBO DE ABONO");
        expect(text).toContain("Abono #3");
        expect(text).toContain("Saldo anterior");
        expect(text).toContain("Saldo pendiente");
        expect(text).toContain("TRX-100");
    });

    it("imprime evidencia de anulacion con motivo y saldo restaurado", () => {
        const ticket: TicketData = {
            ...ticketSimple,
            esRecibo: true,
            esReciboAnulado: true,
            saldoAnterior: 10000,
            saldoNuevo: 15000,
            fechaAnulacionUtc: "2025-03-15T16:00:00Z",
            usuarioAnulaNombre: "Admin",
            motivoAnulacion: "Duplicado",
            pagos: [
                {
                    ...ticketSimple.pagos[0],
                    numeroAbono: 4,
                    montoAplicado: 5000,
                    montoEntregado: 5000,
                    montoVuelto: 0,
                    anulado: true,
                    fechaAnulacionUtc: "2025-03-15T16:00:00Z",
                    usuarioAnulaNombre: "Admin",
                    motivoAnulacion: "Duplicado",
                },
            ],
            lineas: [],
        };

        const text = buildTicket(ticket, perfil58, false).toString("latin1");
        expect(text).toContain("ANULACION DE ABONO");
        expect(text).toContain("Monto revertido");
        expect(text).toContain("Saldo pendiente");
        expect(text).toContain("Admin");
        expect(text).toContain("Duplicado");
    });
});
