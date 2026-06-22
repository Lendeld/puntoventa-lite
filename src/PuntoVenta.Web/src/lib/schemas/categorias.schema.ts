import { z } from "zod";
import { CATEGORIA_FIELDS } from "@lib/constants/categorias.constants";

const NOMBRE_MAX = 150;
const DESCRIPCION_MAX = 255;

export const crearCategoriaSchema = z.object({
    [CATEGORIA_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [CATEGORIA_FIELDS.DESCRIPCION]: z
        .string()
        .trim()
        .max(
            DESCRIPCION_MAX,
            `La descripción no puede exceder ${DESCRIPCION_MAX} caracteres.`,
        )
        .or(z.literal("")),
});

export const actualizarCategoriaSchema = crearCategoriaSchema.extend({
    [CATEGORIA_FIELDS.ACTIVO]: z.boolean(),
});
