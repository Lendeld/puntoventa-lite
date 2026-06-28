import { describe, expect, it } from "vitest";
import { PROVEEDOR_FIELDS, PROVEEDOR_MAX } from "@lib/constants/proveedores.constants";
import {
    actualizarProveedorSchema,
    crearProveedorSchema,
} from "@lib/schemas/proveedores.schema";

const validos = {
    [PROVEEDOR_FIELDS.NOMBRE]: "ACME S.A.",
    [PROVEEDOR_FIELDS.CORREO]: "",
    [PROVEEDOR_FIELDS.TELEFONO]: "",
    [PROVEEDOR_FIELDS.OBSERVACION]: "",
};

describe("proveedores.schema", () => {
    describe("crearProveedorSchema", () => {
        it("acepta solo nombre", () => {
            expect(crearProveedorSchema.safeParse(validos).success).toBe(true);
        });

        it("acepta todos los campos validos", () => {
            const result = crearProveedorSchema.safeParse({
                [PROVEEDOR_FIELDS.NOMBRE]: "Distribuidor CR",
                [PROVEEDOR_FIELDS.CORREO]: "ventas@dist.cr",
                [PROVEEDOR_FIELDS.TELEFONO]: "2222-3333",
                [PROVEEDOR_FIELDS.OBSERVACION]: "Zona norte",
            });
            expect(result.success).toBe(true);
        });

        it("rechaza nombre vacio", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.NOMBRE]: "",
            });
            expect(result.success).toBe(false);
        });

        it("rechaza nombre mayor al maximo", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.NOMBRE]: "N".repeat(PROVEEDOR_MAX.NOMBRE + 1),
            });
            expect(result.success).toBe(false);
        });

        it("rechaza correo con formato invalido", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.CORREO]: "no-es-correo",
            });
            expect(result.success).toBe(false);
        });

        it("acepta correo vacio", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.CORREO]: "",
            });
            expect(result.success).toBe(true);
        });

        it("acepta correo valido", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.CORREO]: "proveedor@ejemplo.com",
            });
            expect(result.success).toBe(true);
        });

        it("rechaza correo mayor al maximo", () => {
            // local + "@b.com" (6 chars) must exceed CORREO_MAX total
            const local = "a".repeat(PROVEEDOR_MAX.CORREO);
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.CORREO]: `${local}@b.com`,
            });
            expect(result.success).toBe(false);
        });

        it("rechaza telefono mayor al maximo", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.TELEFONO]: "1".repeat(PROVEEDOR_MAX.TELEFONO + 1),
            });
            expect(result.success).toBe(false);
        });

        it("rechaza observacion mayor al maximo", () => {
            const result = crearProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.OBSERVACION]: "x".repeat(PROVEEDOR_MAX.OBSERVACION + 1),
            });
            expect(result.success).toBe(false);
        });
    });

    describe("actualizarProveedorSchema", () => {
        it("acepta campos validos con activo", () => {
            const result = actualizarProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.ACTIVO]: true,
            });
            expect(result.success).toBe(true);
        });

        it("acepta activo false (desactivar)", () => {
            const result = actualizarProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.ACTIVO]: false,
            });
            expect(result.success).toBe(true);
        });

        it("rechaza correo invalido igual que crearSchema", () => {
            const result = actualizarProveedorSchema.safeParse({
                ...validos,
                [PROVEEDOR_FIELDS.CORREO]: "maleformado",
                [PROVEEDOR_FIELDS.ACTIVO]: true,
            });
            expect(result.success).toBe(false);
        });
    });
});
