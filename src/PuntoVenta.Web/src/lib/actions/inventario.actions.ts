"use server";

import { AJUSTE_STOCK_FIELDS } from "@lib/constants/inventario.constants";
import { ajusteStockSchema } from "@lib/schemas/inventario.schema";
import { ajustarStockService } from "@lib/services/inventario.service";
import type { ActionResult } from "@lib/types/base.types";
import type { AjusteStockFormValues } from "@lib/schemas/inventario.schema";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function ajustarStockAction(
    values: AjusteStockFormValues,
): Promise<ActionResult> {
    const parsed = ajusteStockSchema.safeParse(values);

    if (!parsed.success) {
        return { status: 400, errors: zodIssuesToErrors(parsed.error.issues) };
    }

    const body = {
        productoId: parsed.data[AJUSTE_STOCK_FIELDS.PRODUCTO_ID],
        delta: parsed.data[AJUSTE_STOCK_FIELDS.DELTA],
        razon: parsed.data[AJUSTE_STOCK_FIELDS.RAZON] || undefined,
    };

    const response = await ajustarStockService(body);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 201, errors: undefined };
}
