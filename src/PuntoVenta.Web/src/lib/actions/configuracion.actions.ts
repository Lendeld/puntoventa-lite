"use server";

import { NEGOCIO_FIELDS } from "@lib/constants/configuracion.constants";
import { actualizarNegocioSchema } from "@lib/schemas/configuracion.schema";
import {
    actualizarNegocioService,
    subirLogoNegocioService,
    toggleEstadoTipoIdentificacionService,
    toggleEstadoCondicionVentaService,
    toggleEstadoMedioPagoService,
    toggleEstadoCodigoImpuestoService,
    toggleEstadoTarifaIvaService,
} from "@lib/services/configuracion.service";
import type { ActionResult, DataAPI } from "@lib/types/base.types";
import type { ActualizarNegocioFormValues } from "@lib/types/configuracion.types";
import { normalizeNullableText, normalizeText } from "@lib/utils/text.utils";
import { zodIssuesToErrors } from "@lib/utils/zodErrors";

export async function subirLogoNegocioAction(
    archivo: File,
): Promise<ActionResult> {
    const response = await subirLogoNegocioService(archivo);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return { status: 200, errors: undefined };
}

export async function actualizarNegocioAction(
    id: string,
    values: ActualizarNegocioFormValues,
): Promise<ActionResult> {
    const parsed = actualizarNegocioSchema.safeParse(values);

    if (!parsed.success) {
        return {
            status: 400,
            errors: zodIssuesToErrors(parsed.error.issues),
        };
    }

    const response = await actualizarNegocioService({
        id,
        nombre: normalizeText(values[NEGOCIO_FIELDS.NOMBRE]),
        nombreComercial: normalizeNullableText(
            values[NEGOCIO_FIELDS.NOMBRE_COMERCIAL],
        ),
        direccion: normalizeText(values[NEGOCIO_FIELDS.DIRECCION]),
        tipoIdentificacionCodigo: values[NEGOCIO_FIELDS.TIPO_IDENTIFICACION_ID],
        identificacion: normalizeText(values[NEGOCIO_FIELDS.IDENTIFICACION]),
        correo: normalizeText(values[NEGOCIO_FIELDS.CORREO]),
        telefono: normalizeNullableText(values[NEGOCIO_FIELDS.TELEFONO]),
        aplicaVendedores: values[NEGOCIO_FIELDS.APLICA_VENDEDORES],
        aplicaCajas: values[NEGOCIO_FIELDS.APLICA_CAJAS],
        tipoCambioPredeterminado: values[NEGOCIO_FIELDS.TIPO_CAMBIO_PREDETERMINADO],
    });

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 204,
        errors: undefined,
    };
}

export async function toggleEstadoTipoIdentificacionAction(
    id: string,
): Promise<ActionResult> {
    const response = await toggleEstadoTipoIdentificacionService(id);

    if (response.errors) {
        return {
            status: response.errors.status,
            errors: response.errors.errors,
        };
    }

    return {
        status: 200,
        errors: undefined,
    };
}

async function toggleCatalogo(
    id: string,
    svc: (id: string) => Promise<DataAPI<unknown>>,
): Promise<ActionResult> {
    const response = await svc(id);
    if (response.errors) {
        return { status: response.errors.status, errors: response.errors.errors };
    }
    return { status: 200, errors: undefined };
}

export async function toggleEstadoCondicionVentaAction(id: string): Promise<ActionResult> {
    return toggleCatalogo(id, toggleEstadoCondicionVentaService);
}

export async function toggleEstadoMedioPagoAction(id: string): Promise<ActionResult> {
    return toggleCatalogo(id, toggleEstadoMedioPagoService);
}

export async function toggleEstadoCodigoImpuestoAction(id: string): Promise<ActionResult> {
    return toggleCatalogo(id, toggleEstadoCodigoImpuestoService);
}

export async function toggleEstadoTarifaIvaAction(id: string): Promise<ActionResult> {
    return toggleCatalogo(id, toggleEstadoTarifaIvaService);
}
