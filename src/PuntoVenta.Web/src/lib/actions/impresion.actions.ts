"use server";

import { actualizarNegocioTicketConfigService } from "@lib/services/impresion.service";
import { actualizarNegocioTicketConfigSchema } from "@lib/schemas/negocio-ticket-config.schema";
import type { ActualizarNegocioTicketConfigFormValues } from "@lib/types/impresion.types";
import { ActionResult } from "@lib/types/base.types";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function actualizarNegocioTicketConfigAction(
    values: ActualizarNegocioTicketConfigFormValues,
): Promise<ActionResult> {
    const parsed = actualizarNegocioTicketConfigSchema.safeParse(values);
    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await actualizarNegocioTicketConfigService(parsed.data);
    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return { status: 204, errors: undefined };
}
