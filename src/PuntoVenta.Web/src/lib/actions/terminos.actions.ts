"use server";

import { aceptarTerminosService } from "@lib/services/terminos.service";
import type { ActionResult } from "@lib/types/base.types";

export async function aceptarTerminosAction(version: string): Promise<ActionResult> {
    const response = await aceptarTerminosService(version);

    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }

    return { status: 200, errors: undefined };
}
