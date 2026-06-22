import { describe, expect, it } from "vitest";
import { ajusteStockSchema } from "@lib/schemas/inventario.schema";
import { AJUSTE_STOCK_FIELDS } from "@lib/constants/inventario.constants";

const BASE_VALID = {
    [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f",
    [AJUSTE_STOCK_FIELDS.DELTA]: 10,
    [AJUSTE_STOCK_FIELDS.RAZON]: "Ingreso inicial",
};

describe("inventario.schema — ajusteStockSchema", () => {
    it("acepta datos válidos", () => {
        expect(ajusteStockSchema.safeParse(BASE_VALID).success).toBe(true);
    });

    it("acepta delta negativo (salida de stock)", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.DELTA]: -5,
        });
        expect(result.success).toBe(true);
    });

    it("acepta razon undefined (campo opcional)", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.RAZON]: undefined,
        });
        expect(result.success).toBe(true);
    });

    it("rechaza productoId vacío", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: "",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza productoId que no es UUID", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: "no-es-uuid",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza delta igual a cero", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.DELTA]: 0,
        });
        expect(result.success).toBe(false);
        if (!result.success) {
            const deltaIssue = result.error.issues.find((i) =>
                i.path.includes(AJUSTE_STOCK_FIELDS.DELTA),
            );
            expect(deltaIssue?.message).toBe("La cantidad no puede ser cero.");
        }
    });

    it("rechaza razon mayor a 255 caracteres", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.RAZON]: "R".repeat(256),
        });
        expect(result.success).toBe(false);
    });

    it("acepta razon de exactamente 255 caracteres", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.RAZON]: "R".repeat(255),
        });
        expect(result.success).toBe(true);
    });

    it("acepta delta decimal (fracción)", () => {
        const result = ajusteStockSchema.safeParse({
            ...BASE_VALID,
            [AJUSTE_STOCK_FIELDS.DELTA]: 0.5,
        });
        expect(result.success).toBe(true);
    });
});
