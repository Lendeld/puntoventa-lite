import { z } from "zod";
import { ROL_FIELDS } from "@lib/constants/roles.constants";

const NOMBRE_MAX = 100;
const DESCRIPCION_MAX = 500;

export const crearRolSchema = z.object({
    [ROL_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(
            NOMBRE_MAX,
            `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`,
        ),
    [ROL_FIELDS.DESCRIPCION]: z
        .string()
        .trim()
        .max(
            DESCRIPCION_MAX,
            `La descripción no puede exceder ${DESCRIPCION_MAX} caracteres.`,
        )
        .or(z.literal("")),
});

export const actualizarRolSchema = z.object({
    [ROL_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(
            NOMBRE_MAX,
            `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`,
        ),
    [ROL_FIELDS.DESCRIPCION]: z
        .string()
        .trim()
        .max(
            DESCRIPCION_MAX,
            `La descripción no puede exceder ${DESCRIPCION_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [ROL_FIELDS.ACTIVO]: z.boolean(),
});
