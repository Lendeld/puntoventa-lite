import { describe, expect, it } from "vitest";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";
import { crearProductoSchema, editarProductoSchema } from "@lib/schemas/productos.schema";

const BASE_VALID = {
    [PRODUCTO_FIELDS.CODIGO]: "ABC-001",
    [PRODUCTO_FIELDS.CODIGO_BARRAS]: "",
    [PRODUCTO_FIELDS.NOMBRE]: "Producto Test",
    [PRODUCTO_FIELDS.DESCRIPCION]: "",
    [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
    [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 100,
    [PRODUCTO_FIELDS.PRECIO_COSTO]: undefined,
    [PRODUCTO_FIELDS.CATEGORIA_ID]: "3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f",
    [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: "08",
    [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: false,
    [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: false,
};

describe("productos.schema — crearProductoSchema", () => {
    it("acepta datos válidos con tarifa IVA", () => {
        expect(crearProductoSchema.safeParse(BASE_VALID).success).toBe(true);
    });

    it("rechaza tarifa IVA vacía", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: "",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza tarifa IVA null", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: null,
        });
        expect(result.success).toBe(false);
    });

    it("rechaza codigo vacío", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.CODIGO]: "",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza codigo mayor a 20 caracteres", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.CODIGO]: "A".repeat(21),
        });
        expect(result.success).toBe(false);
    });

    it("rechaza nombre vacío", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.NOMBRE]: "",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza nombre mayor a 150 caracteres", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.NOMBRE]: "N".repeat(151),
        });
        expect(result.success).toBe(false);
    });

    it("rechaza precio unitario <= 0 para bien (tipo_item 1)", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
            [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 0,
        });
        expect(result.success).toBe(false);
    });

    it("rechaza precio costo negativo", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.PRECIO_COSTO]: -1,
        });
        expect(result.success).toBe(false);
    });

    it("rechaza categoriaId sin formato UUID", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.CATEGORIA_ID]: "no-uuid",
        });
        expect(result.success).toBe(false);
    });

    it("acepta precio costo opcional undefined", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.PRECIO_COSTO]: undefined,
        });
        expect(result.success).toBe(true);
    });

    it("acepta codigo de barras vacío", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.CODIGO_BARRAS]: "",
        });
        expect(result.success).toBe(true);
    });

    it("acepta permiteModificarPrecioUnitario true", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: true,
        });
        expect(result.success).toBe(true);
    });

    it("permiteModificarPrecioUnitario usa false por defecto", () => {
        const result = crearProductoSchema.safeParse(BASE_VALID);
        expect(result.success).toBe(true);
        expect(result.data?.[PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]).toBe(false);
    });

    it("bien (tipo_item 1) con noAplicaExistencias = true acepta precio > 0", () => {
        const result = crearProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.TIPO_ITEM]: 1,
            [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: true,
            [PRODUCTO_FIELDS.PRECIO_UNITARIO]: 100,
        });
        expect(result.success).toBe(true);
    });
});

describe("productos.schema — editarProductoSchema", () => {
    it("acepta datos válidos", () => {
        expect(editarProductoSchema.safeParse(BASE_VALID).success).toBe(true);
    });

    it("acepta datos con tarifa IVA", () => {
        const result = editarProductoSchema.safeParse({
            ...BASE_VALID,
            [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: "08",
        });
        expect(result.success).toBe(true);
    });
});
