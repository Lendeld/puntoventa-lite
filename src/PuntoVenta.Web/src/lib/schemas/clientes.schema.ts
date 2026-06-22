import { z } from "zod";
import { CLIENTE_FIELDS } from "@lib/constants/clientes.constants";

const NOMBRE_MAX = 100;
const IDENTIFICACION_MAX = 20;
const CORREO_MAX = 160;
const TELEFONO_MAX = 20;
const OBSERVACIONES_MAX = 500;

export const crearClienteSchema = z.object({
    [CLIENTE_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [CLIENTE_FIELDS.IDENTIFICACION]: z
        .string()
        .trim()
        .max(
            IDENTIFICACION_MAX,
            `La identificación no puede exceder ${IDENTIFICACION_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [CLIENTE_FIELDS.CORREO]: z
        .string()
        .trim()
        .max(CORREO_MAX, `El correo no puede exceder ${CORREO_MAX} caracteres.`)
        .refine(
            (value) => value === "" || z.email().safeParse(value).success,
            "El correo no tiene un formato válido.",
        ),
    [CLIENTE_FIELDS.TELEFONO]: z
        .string()
        .trim()
        .max(
            TELEFONO_MAX,
            `El teléfono no puede exceder ${TELEFONO_MAX} caracteres.`,
        )
        .or(z.literal("")),
    [CLIENTE_FIELDS.OBSERVACIONES]: z
        .string()
        .trim()
        .max(
            OBSERVACIONES_MAX,
            `Las observaciones no pueden exceder ${OBSERVACIONES_MAX} caracteres.`,
        )
        .or(z.literal("")),
});

export const actualizarClienteSchema = crearClienteSchema.extend({
    [CLIENTE_FIELDS.ACTIVO]: z.boolean(),
});
