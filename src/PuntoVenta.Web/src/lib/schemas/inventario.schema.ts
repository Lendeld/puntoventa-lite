import { z } from "zod";
import { AJUSTE_STOCK_FIELDS } from "@lib/constants/inventario.constants";

// RazonMaxLength = 255 en Domain/Entities/MovimientosStock/MovimientoStock.cs
const RAZON_MAX = 255;

export const ajusteStockSchema = z.object({
    [AJUSTE_STOCK_FIELDS.PRODUCTO_ID]: z.uuid("ID de producto inválido."),
    [AJUSTE_STOCK_FIELDS.DELTA]: z
        .number({ message: "La cantidad es requerida." })
        .refine((v) => v !== 0, { message: "La cantidad no puede ser cero." }),
    [AJUSTE_STOCK_FIELDS.RAZON]: z
        .string()
        .max(RAZON_MAX, `La razón no puede exceder ${RAZON_MAX} caracteres.`)
        .optional(),
});

export type AjusteStockFormValues = z.infer<typeof ajusteStockSchema>;
