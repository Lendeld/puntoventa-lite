import { z } from "zod";
import { CAJA_FIELDS, CAJA_MAX } from "@lib/constants/cajas.constants";

export const crearCajaSchema = z.object({
    [CAJA_FIELDS.CODIGO]: z
        .string()
        .trim()
        .min(1, "El código es requerido.")
        .max(CAJA_MAX.CODIGO, `Máximo ${CAJA_MAX.CODIGO} caracteres.`),
    [CAJA_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(CAJA_MAX.NOMBRE, `Máximo ${CAJA_MAX.NOMBRE} caracteres.`),
});

export const actualizarCajaSchema = crearCajaSchema;
