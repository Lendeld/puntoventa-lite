import { describe, it, expect } from "vitest";
import { establecerPinSchema } from "@lib/schemas/auth.schema";
import { ESTABLECER_PIN_FIELDS } from "@lib/constants/auth.constants";

describe("establecerPinSchema", () => {
    const valido = {
        [ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL]: "MiPassword123!",
        [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "123456",
        [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "123456",
    };

    it("acepta un PIN de exactamente 6 dígitos", () => {
        expect(establecerPinSchema.safeParse(valido).success).toBe(true);
    });

    it("rechaza un PIN de 5 dígitos", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "12345",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "12345",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza un PIN de 7 dígitos", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "1234567",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "1234567",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza un PIN con letras", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "abcdef",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "abcdef",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza un PIN con caracteres especiales", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "12345!",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "12345!",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza un PIN vacío", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza cuando los PINs no coinciden", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "123456",
            [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "654321",
        });
        expect(result.success).toBe(false);
    });

    it("rechaza cuando la contraseña actual está vacía", () => {
        const result = establecerPinSchema.safeParse({
            ...valido,
            [ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL]: "",
        });
        expect(result.success).toBe(false);
    });

    it("acepta el PIN 000000 (todo ceros)", () => {
        expect(
            establecerPinSchema.safeParse({
                ...valido,
                [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "000000",
                [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "000000",
            }).success,
        ).toBe(true);
    });

    it("acepta el PIN 999999 (todo nueves)", () => {
        expect(
            establecerPinSchema.safeParse({
                ...valido,
                [ESTABLECER_PIN_FIELDS.PIN_NUEVO]: "999999",
                [ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO]: "999999",
            }).success,
        ).toBe(true);
    });
});

describe("ESTABLECER_PIN_FIELDS constantes", () => {
    it("PASSWORD_ACTUAL usa el formato campo de formulario correcto", () => {
        expect(ESTABLECER_PIN_FIELDS.PASSWORD_ACTUAL).toBe("Usuario_PasswordActual");
    });

    it("PIN_NUEVO usa el formato campo de formulario correcto", () => {
        expect(ESTABLECER_PIN_FIELDS.PIN_NUEVO).toBe("Usuario_Pin");
    });

    it("CONFIRMAR_PIN_NUEVO usa el formato campo de formulario correcto", () => {
        expect(ESTABLECER_PIN_FIELDS.CONFIRMAR_PIN_NUEVO).toBe("Usuario_ConfirmarPin");
    });
});
