import { describe, expect, it } from "vitest";
import { NEGOCIO_TICKET_CONFIG_FIELDS } from "@lib/constants/negocio-ticket-config.constants";
import { actualizarNegocioTicketConfigSchema } from "@lib/schemas/negocio-ticket-config.schema";

const F = NEGOCIO_TICKET_CONFIG_FIELDS;

const TIPOS_FIJOS = [
    "NombreComercial",
    "Nombre",
    "IdentificacionFiscal",
    "Telefono",
    "Correo",
    "Direccion",
    "Fecha",
] as const;

function encabezadoCompleto(extra: unknown[] = []) {
    return [
        ...TIPOS_FIJOS.map((tipo) => ({
            _key: crypto.randomUUID(),
            tipo,
            visible: true,
            textoLibre: null,
        })),
        ...extra,
    ];
}

function base(configuraciones: unknown[], elementosEncabezado: unknown[] = encabezadoCompleto()) {
    return {
        [F.MENSAJE_PIE]: "",
        [F.MOSTRAR_LOGO]: true,
        [F.APLICA_COPIA_CLIENTE_NEGOCIO]: false,
        [F.MOSTRAR_CODIGO_BARRAS]: true,
        [F.CONFIGURACIONES]: configuraciones,
        [F.ELEMENTOS_ENCABEZADO]: elementosEncabezado,
    };
}

function config(over: Record<string, unknown> = {}) {
    return {
        _key: crypto.randomUUID(),
        nombre: "Facturas",
        destino: "Pdf",
        tiposDocumento: ["Factura"],
        lineas: [
            {
                _key: crypto.randomUUID(),
                texto: "SINPE 8888-8888",
                alineacion: "Centro",
                negrita: false,
            },
        ],
        ...over,
    };
}

describe("negocio-ticket-config.schema", () => {
    it("acepta configuraciones válidas", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([config()]),
        );
        expect(result.success).toBe(true);
    });

    it("rechaza nombre de configuración vacío", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([config({ nombre: "   " })]),
        );
        expect(result.success).toBe(false);
    });

    it("rechaza línea con texto vacío", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([
                config({
                    lineas: [
                        {
                            _key: crypto.randomUUID(),
                            texto: "  ",
                            alineacion: "Izquierda",
                            negrita: false,
                        },
                    ],
                }),
            ]),
        );
        expect(result.success).toBe(false);
    });

    it("rechaza tipos traslapados en el mismo destino", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([
                config({ nombre: "A", tiposDocumento: ["Factura"] }),
                config({ nombre: "B", tiposDocumento: ["Factura"] }),
            ]),
        );
        expect(result.success).toBe(false);
    });

    it("permite el mismo tipo en destinos distintos", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([
                config({ destino: "Pdf", tiposDocumento: ["Factura"] }),
                config({ destino: "Ticket", tiposDocumento: ["Factura"] }),
            ]),
        );
        expect(result.success).toBe(true);
    });

    it("rechaza 'Todos' junto a otra configuración del mismo destino", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([
                config({ nombre: "Todos", tiposDocumento: [] }),
                config({ nombre: "Factura", tiposDocumento: ["Factura"] }),
            ]),
        );
        expect(result.success).toBe(false);
    });

    it("acepta encabezado con un texto libre", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base(
                [config()],
                encabezadoCompleto([
                    {
                        _key: crypto.randomUUID(),
                        tipo: "Texto",
                        visible: true,
                        textoLibre: "Gracias por su compra",
                    },
                ]),
            ),
        );
        expect(result.success).toBe(true);
    });

    it("rechaza encabezado sin un tipo fijo", () => {
        const incompleto = encabezadoCompleto().slice(1); // quita NombreComercial
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base([config()], incompleto),
        );
        expect(result.success).toBe(false);
    });

    it("rechaza elemento de texto sin texto", () => {
        const result = actualizarNegocioTicketConfigSchema.safeParse(
            base(
                [config()],
                encabezadoCompleto([
                    {
                        _key: crypto.randomUUID(),
                        tipo: "Texto",
                        visible: true,
                        textoLibre: "   ",
                    },
                ]),
            ),
        );
        expect(result.success).toBe(false);
    });
});
