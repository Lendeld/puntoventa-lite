"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";
import type {
    ObtenerMovimientosStockParams,
    ObtenerMovimientosStockResult,
} from "@lib/types/inventario.types";

interface AjustarStockServiceParams {
    productoId: string;
    delta: number;
    razon?: string;
}

export async function obtenerMovimientosStockService(
    params: ObtenerMovimientosStockParams,
): Promise<DataAPI<ObtenerMovimientosStockResult>> {
    return await requestAPI<ObtenerMovimientosStockResult>({
        url: "/inventario/movimientos-stock",
        method: "GET",
        query: {
            productoId: params.productoId,
            pagina: params.pagina,
            tamano: params.tamano,
        },
    });
}

export async function ajustarStockService(
    body: AjustarStockServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/inventario/ajuste-stock",
        method: "POST",
        body,
    });
}
