import { describe, expect, it } from "vitest";
import { actualizarNegocioSchema } from "@lib/schemas/configuracion.schema";
import { NEGOCIO_FIELDS } from "@lib/constants/configuracion.constants";

describe("actualizarNegocioSchema", () => {
    const valido = {
        [NEGOCIO_FIELDS.NOMBRE]: "Mi Negocio",
        [NEGOCIO_FIELDS.NOMBRE_COMERCIAL]: "Sucursal Centro",
        [NEGOCIO_FIELDS.DIRECCION]: "San Jose, Avenida Central, local 1",
        [NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID]: "01",
        [NEGOCIO_FIELDS.IDENTIFICACION]: "123456789",
        [NEGOCIO_FIELDS.TELEFONO]: "2222-2222",
        [NEGOCIO_FIELDS.CORREO]: "negocio@demo.com",
        [NEGOCIO_FIELDS.APLICA_VENDEDORES]: false,
        [NEGOCIO_FIELDS.APLICA_CAJAS]: false,
        [NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO]: 500,
    };

    it("valida payload correcto", () => {
        expect(actualizarNegocioSchema.safeParse(valido).success).toBe(true);
    });

    it("valida con aplica_cajas true", () => {
        const result = actualizarNegocioSchema.safeParse({
            ...valido,
            [NEGOCIO_FIELDS.APLICA_CAJAS]: true,
        });
        expect(result.success).toBe(true);
    });

    it("falla si direccion vacia", () => {
        const result = actualizarNegocioSchema.safeParse({
            ...valido,
            [NEGOCIO_FIELDS.DIRECCION]: "",
        });

        expect(result.success).toBe(false);
    });

    it("falla si direccion supera 255 chars", () => {
        const result = actualizarNegocioSchema.safeParse({
            ...valido,
            [NEGOCIO_FIELDS.DIRECCION]: "a".repeat(256),
        });

        expect(result.success).toBe(false);
    });

    it("falla si correo invalido", () => {
        const result = actualizarNegocioSchema.safeParse({
            ...valido,
            [NEGOCIO_FIELDS.CORREO]: "no-es-email",
        });

        expect(result.success).toBe(false);
    });

    it("falla si nombre vacio", () => {
        const result = actualizarNegocioSchema.safeParse({
            ...valido,
            [NEGOCIO_FIELDS.NOMBRE]: "",
        });

        expect(result.success).toBe(false);
    });
});
