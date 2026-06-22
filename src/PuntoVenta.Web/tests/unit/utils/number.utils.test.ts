import { describe, expect, it } from "vitest";
import { redondear, redondearMoneda } from "@lib/utils/number.utils";

describe("redondear", () => {
    it("no cambia entero", () => {
        expect(redondear(100)).toBe(100);
    });

    it("preserva 5 decimales exactos", () => {
        expect(redondear(1.12345)).toBe(1.12345);
    });

    it("redondea al 5to decimal (hacia arriba)", () => {
        expect(redondear(1.123456)).toBe(1.12346);
    });

    it("redondea al 5to decimal (hacia abajo)", () => {
        expect(redondear(1.123454)).toBe(1.12345);
    });

    it("calcula precio venta IVA 13% sin error flotante", () => {
        const unitario = 100;
        const resultado = redondear(unitario * 1.13);
        expect(resultado).toBe(113);
    });

    it("calcula precio unitario desde venta IVA 13%", () => {
        const venta = 113;
        const resultado = redondear(venta / 1.13);
        expect(resultado).toBe(100);
    });

    it("calcula precio venta IVA 4%", () => {
        const resultado = redondear(200 * 1.04);
        expect(resultado).toBe(208);
    });

    it("retorna 0 para entrada 0", () => {
        expect(redondear(0)).toBe(0);
    });
});

describe("redondearMoneda", () => {
    it("redondea a 2 decimales half-away-from-zero", () => {
        // 11.50 * 13% = 1.495 → 1.50 (igual que backend AwayFromZero)
        expect(redondearMoneda(1.495)).toBe(1.5);
    });

    it("no altera montos ya a 2 decimales", () => {
        expect(redondearMoneda(13)).toBe(13);
        expect(redondearMoneda(12.99)).toBe(12.99);
    });

    it("redondea negativos away-from-zero", () => {
        expect(redondearMoneda(-1.495)).toBe(-1.5);
    });

    it("retorna 0 para entrada 0", () => {
        expect(redondearMoneda(0)).toBe(0);
    });
});
