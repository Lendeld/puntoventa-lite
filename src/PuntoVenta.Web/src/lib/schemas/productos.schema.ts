import { z } from "zod";
import { PRODUCTO_FIELDS } from "@lib/constants/productos.constants";

const CODIGO_MAX = 20;
const CODIGO_BARRAS_MAX = 50;
const NOMBRE_MAX = 150;
const DESCRIPCION_MAX = 500;

const baseSchema = z.object({
    [PRODUCTO_FIELDS.CODIGO]: z
        .string()
        .trim()
        .min(1, "El código es requerido.")
        .max(CODIGO_MAX, `El código no puede exceder ${CODIGO_MAX} caracteres.`),
    [PRODUCTO_FIELDS.CODIGO_BARRAS]: z
        .string()
        .trim()
        .max(CODIGO_BARRAS_MAX, `El código de barras no puede exceder ${CODIGO_BARRAS_MAX} caracteres.`)
        .or(z.literal("")),
    [PRODUCTO_FIELDS.NOMBRE]: z
        .string()
        .trim()
        .min(1, "El nombre es requerido.")
        .max(NOMBRE_MAX, `El nombre no puede exceder ${NOMBRE_MAX} caracteres.`),
    [PRODUCTO_FIELDS.DESCRIPCION]: z
        .string()
        .trim()
        .max(DESCRIPCION_MAX, `La descripción no puede exceder ${DESCRIPCION_MAX} caracteres.`)
        .or(z.literal("")),
    [PRODUCTO_FIELDS.TIPO_ITEM]: z
        .number({ message: "El tipo de item es requerido." })
        .int()
        .min(1)
        .max(2),
    [PRODUCTO_FIELDS.PRECIO_UNITARIO]: z
        .number({ message: "El precio unitario es requerido." })
        .min(0, "El precio unitario no puede ser negativo.")
        .nullable()
        .optional(),
    [PRODUCTO_FIELDS.PRECIO_COSTO]: z
        .number()
        .min(0, "El precio de costo no puede ser negativo.")
        .nullable()
        .optional(),
    [PRODUCTO_FIELDS.CATEGORIA_ID]: z
        .union([z.string().uuid(), z.literal("")])
        .optional(),
    [PRODUCTO_FIELDS.TARIFA_IVA_IMPUESTO_CODIGO]: z
        .string()
        .nullable()
        .optional(),
    [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]: z.boolean().default(false),
    [PRODUCTO_FIELDS.PERMITE_MODIFICAR_PRECIO_UNITARIO]: z.boolean().default(false),
    [PRODUCTO_FIELDS.EXISTENCIA_INICIAL]: z
        .number()
        .min(0, "La existencia inicial no puede ser negativa.")
        .nullable()
        .optional(),
});

const refine = (data: z.infer<typeof baseSchema>, ctx: z.RefinementCtx) => {
    const precioUnitario = data[PRODUCTO_FIELDS.PRECIO_UNITARIO];
    if (precioUnitario == null || precioUnitario <= 0) {
        ctx.addIssue({
            code: "custom",
            message: "El precio unitario debe ser mayor a 0.",
            path: [PRODUCTO_FIELDS.PRECIO_UNITARIO],
        });
    }

    if (
        data[PRODUCTO_FIELDS.TIPO_ITEM] !== 1 &&
        data[PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS]
    ) {
        ctx.addIssue({
            code: "custom",
            message: "No aplica existencias solo está disponible para productos tipo bien.",
            path: [PRODUCTO_FIELDS.NO_APLICA_EXISTENCIAS],
        });
    }
};

export const crearProductoSchema = baseSchema.superRefine(refine);

export const editarProductoSchema = baseSchema.superRefine(refine);
