import { describe, expect, it } from "vitest";
import { CAJA_FIELDS, CAJA_MAX } from "@lib/constants/cajas.constants";
import {
    actualizarCajaSchema,
    crearCajaSchema,
} from "@lib/schemas/cajas.schema";

describe("cajas.schema", () => {
    describe("crearCajaSchema", () => {
        it("acepta codigo y nombre validos", () => {
            const result = crearCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "POS01",
                [CAJA_FIELDS.NOMBRE]: "Caja principal",
            });

            expect(result.success).toBe(true);
        });

        it("rechaza codigo vacio", () => {
            const result = crearCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "   ",
                [CAJA_FIELDS.NOMBRE]: "Caja principal",
            });

            expect(result.success).toBe(false);
        });

        it("rechaza nombre vacio", () => {
            const result = crearCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "POS01",
                [CAJA_FIELDS.NOMBRE]: "",
            });

            expect(result.success).toBe(false);
        });

        it("rechaza codigo mayor al maximo", () => {
            const result = crearCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "A".repeat(CAJA_MAX.CODIGO + 1),
                [CAJA_FIELDS.NOMBRE]: "Caja principal",
            });

            expect(result.success).toBe(false);
        });

        it("rechaza nombre mayor al maximo", () => {
            const result = crearCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "POS01",
                [CAJA_FIELDS.NOMBRE]: "N".repeat(CAJA_MAX.NOMBRE + 1),
            });

            expect(result.success).toBe(false);
        });
    });

    describe("actualizarCajaSchema", () => {
        it("acepta mismas reglas que crearCajaSchema", () => {
            const result = actualizarCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "POS02",
                [CAJA_FIELDS.NOMBRE]: "Caja secundaria",
            });

            expect(result.success).toBe(true);
        });

        it("rechaza campos invalidos", () => {
            const result = actualizarCajaSchema.safeParse({
                [CAJA_FIELDS.CODIGO]: "",
                [CAJA_FIELDS.NOMBRE]: "Caja secundaria",
            });

            expect(result.success).toBe(false);
        });
    });
});
