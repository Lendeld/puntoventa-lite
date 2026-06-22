"use server";

import type {
    ActualizarNegocioTicketConfigFormValues,
    NegocioTicketConfigDto,
    PerfilImpresoraTicketDto,
} from "@lib/types/impresion.types";
import type { TicketDataDto } from "@lib/types/ventas.types";
import { requestAPI } from "@lib/utils/requestApi";
import { DataAPI } from "@lib/types/base.types";
import { NEGOCIO_TICKET_CONFIG_FIELDS } from "@lib/constants/negocio-ticket-config.constants";

export async function obtenerNegocioTicketConfigService(): Promise<DataAPI<NegocioTicketConfigDto>> {
    return await requestAPI<NegocioTicketConfigDto>({
        url: "/negocio/ticket-config",
        method: "GET",
    });
}

export async function actualizarNegocioTicketConfigService(
    body: ActualizarNegocioTicketConfigFormValues,
): Promise<DataAPI<null>> {
    // El command del backend espera nombres planos (MensajePie, MostrarLogo, …).
    // Las claves del form llevan el prefijo "NegocioTicketConfig_" y no bindean
    // por nombre, así que se remapean aquí (mismo patrón que configuracion.service).
    return await requestAPI<null>({
        url: "/negocio/ticket-config",
        method: "PUT",
        body: {
            mensajePie: body[NEGOCIO_TICKET_CONFIG_FIELDS.MENSAJE_PIE],
            mostrarLogo: body[NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_LOGO],
            aplicaCopiaClienteNegocio:
                body[NEGOCIO_TICKET_CONFIG_FIELDS.APLICA_COPIA_CLIENTE_NEGOCIO],
            mostrarCodigoBarras:
                body[NEGOCIO_TICKET_CONFIG_FIELDS.MOSTRAR_CODIGO_BARRAS],
            configuraciones:
                body[NEGOCIO_TICKET_CONFIG_FIELDS.CONFIGURACIONES],
            elementosEncabezado:
                body[NEGOCIO_TICKET_CONFIG_FIELDS.ELEMENTOS_ENCABEZADO],
        },
    });
}

export async function obtenerTicketDataService(
    documentoId: string,
    pagoId?: string,
): Promise<DataAPI<TicketDataDto>> {
    const url = pagoId
        ? `/ventas/${documentoId}/ticket-data?pagoId=${pagoId}`
        : `/ventas/${documentoId}/ticket-data`;
    return await requestAPI<TicketDataDto>({ url, method: "GET" });
}

export async function obtenerPerfilesImpresoraService(): Promise<DataAPI<PerfilImpresoraTicketDto[]>> {
    return await requestAPI<PerfilImpresoraTicketDto[]>({
        url: "/impresion/perfiles",
        method: "GET",
    });
}
