import { describe, expect, it } from "vitest";
import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";
import {
    actualizarCategoriaSchema,
    crearCategoriaSchema,
} from "@lib/schemas/categorias.schema";

describe("categorias.schema", () => {
    it("acepta nombre y descripcion validos", () => {
        const result = crearCategoriaSchema.safeParse({
            [CATEGORIA_FIELDS.NOMBRE]: "Categoria Demo",
            [CATEGORIA_FIELDS.DESCRIPCION]: "Descripcion",
        });

        expect(result.success).toBe(true);
    });

    it("rechaza nombre vacio", () => {
        const result = crearCategoriaSchema.safeParse({
            [CATEGORIA_FIELDS.NOMBRE]: "   ",
            [CATEGORIA_FIELDS.DESCRIPCION]: "",
        });

        expect(result.success).toBe(false);
    });

    it("actualizarCategoriaSchema requiere activo boolean", () => {
        const result = actualizarCategoriaSchema.safeParse({
            [CATEGORIA_FIELDS.NOMBRE]: "Categoria Demo",
            [CATEGORIA_FIELDS.DESCRIPCION]: "",
            [CATEGORIA_FIELDS.ACTIVO]: true,
        });

        expect(result.success).toBe(true);
    });
});
