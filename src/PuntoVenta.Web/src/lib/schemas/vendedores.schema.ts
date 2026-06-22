import { z } from "zod";
import { VENDEDOR_FIELDS } from "@lib/constants/vendedores.constants";

const NOMBRE_MAX = 150;

export const crearVendedorSchema = z.object({
    [VENDEDOR_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [VENDEDOR_FIELDS.IS_PRINCIPAL]: z.boolean(),
});

export const actualizarVendedorSchema = crearVendedorSchema.extend({
    [VENDEDOR_FIELDS.ACTIVO]: z.boolean(),
});
