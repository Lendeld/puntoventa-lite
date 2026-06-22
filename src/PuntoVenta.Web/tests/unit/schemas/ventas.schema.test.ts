import { describe, expect, it } from "vitest";
import { crearBorradorFacturaSchema, crearFacturaSchema } from "@lib/schemas/ventas.schema";
import { VENTA_FIELDS } from "@lib/constants/ventas.constants";

function buildValidValues() {
    return {
        [VENTA_FIELDS.CLIENTE_ID]: "",
        [VENTA_FIELDS.VENDEDOR_ID]: "",
        [VENTA_FIELDS.CAJA_ID]: "",
        [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: "01",
        [VENTA_FIELDS.FECHA_DOCUMENTO]: "2026-05-01",
        [VENTA_FIELDS.MONEDA_CODIGO]: "CRC",
        [VENTA_FIELDS.TIPO_CAMBIO]: 1,
        [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: null,
        [VENTA_FIELDS.OBSERVACIONES]: "",
        [VENTA_FIELDS.LINEAS]: [
            {
                ProductoId: "0f0a61b3-5cc4-4dd4-92f2-2f7da4126c12",
                TipoItem: "Bien",
                Codigo: "P-001",
                Descripcion: "Producto 1",
                Cantidad: 1,
                PrecioUnitario: 1000,
                MontoDescuento: 0,
                TarifaIvaImpuestoCodigo: "08",
                PorcentajeImpuesto: 13,
            },
        ],
        [VENTA_FIELDS.PAGOS]: [
            {
                MonedaCodigo: "CRC",
                TipoCambioAplicado: 1,
                MedioPagoCodigo: "01",
                MontoEntregado: 1130,
                MontoAplicadoMonedaPago: 1130,
                MontoAplicadoDocumento: 1130,
                MontoVueltoMonedaPago: 0,
                MontoVueltoDocumento: 0,
                Referencia: "",
                Observacion: "",
            },
        ],
    };
}

describe("ventas.schema", () => {
    it("requiere al menos una línea", () => {
        const result = crearBorradorFacturaSchema.safeParse({
            ...buildValidValues(),
            [VENTA_FIELDS.LINEAS]: [],
        });

        expect(result.success).toBe(false);
        expect(result.error?.issues.some((issue) => issue.path[0] === VENTA_FIELDS.LINEAS)).toBe(true);
    });

    it("borrador contado permite seguir sin pagos", () => {
        const result = crearBorradorFacturaSchema.safeParse({
            ...buildValidValues(),
            [VENTA_FIELDS.PAGOS]: [],
        });

        expect(result.success).toBe(true);
    });

    it("factura contado requiere pagos suficientes", () => {
        const result = crearFacturaSchema.safeParse({
            ...buildValidValues(),
            [VENTA_FIELDS.PAGOS]: [
                {
                    MonedaCodigo: "CRC",
                    TipoCambioAplicado: 1,
                    MedioPagoCodigo: "01",
                    MontoEntregado: 500,
                    MontoAplicadoMonedaPago: 500,
                    MontoAplicadoDocumento: 500,
                    MontoVueltoMonedaPago: 0,
                    MontoVueltoDocumento: 0,
                    Referencia: "",
                    Observacion: "",
                },
            ],
        });

        expect(result.success).toBe(false);
        expect(result.error?.issues.some((issue) => issue.path[0] === VENTA_FIELDS.PAGOS)).toBe(true);
    });

    it("crédito requiere plazo válido", () => {
        const result = crearBorradorFacturaSchema.safeParse({
            ...buildValidValues(),
            [VENTA_FIELDS.CONDICION_VENTA_CODIGO]: "02",
            [VENTA_FIELDS.PLAZO_CREDITO_DIAS]: null,
            [VENTA_FIELDS.PAGOS]: [],
        });

        expect(result.success).toBe(false);
        expect(result.error?.issues.some((issue) => issue.path[0] === VENTA_FIELDS.PLAZO_CREDITO_DIAS)).toBe(true);
    });

    it("acepta caja_id vacío (opcional)", () => {
        const result = crearBorradorFacturaSchema.safeParse({
            ...buildValidValues(),
            [VENTA_FIELDS.CAJA_ID]: "",
        });

        expect(result.success).toBe(true);
    });
});
