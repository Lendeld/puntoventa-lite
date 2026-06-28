import { z } from "zod";
import { PROVEEDOR_FIELDS, PROVEEDOR_MAX } from "@lib/constants/proveedores.constants";

export const crearProveedorSchema = z.object({
    [PROVEEDOR_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(PROVEEDOR_MAX.NOMBRE, `El nombre no puede exceder ${PROVEEDOR_MAX.NOMBRE} caracteres.`),
    [PROVEEDOR_FIELDS.CORREO]: z
        .string()
        .trim()
        .max(PROVEEDOR_MAX.CORREO, `El correo no puede exceder ${PROVEEDOR_MAX.CORREO} caracteres.`)
        .refine(
            (value) => value === "" || z.email().safeParse(value).success,
            "El correo no tiene un formato válido.",
        ),
    [PROVEEDOR_FIELDS.TELEFONO]: z
        .string()
        .trim()
        .max(PROVEEDOR_MAX.TELEFONO, `El teléfono no puede exceder ${PROVEEDOR_MAX.TELEFONO} caracteres.`)
        .or(z.literal("")),
    [PROVEEDOR_FIELDS.OBSERVACION]: z
        .string()
        .trim()
        .max(PROVEEDOR_MAX.OBSERVACION, `La observación no puede exceder ${PROVEEDOR_MAX.OBSERVACION} caracteres.`)
        .or(z.literal("")),
});

export const actualizarProveedorSchema = crearProveedorSchema.extend({
    [PROVEEDOR_FIELDS.ACTIVO]: z.boolean(),
});
