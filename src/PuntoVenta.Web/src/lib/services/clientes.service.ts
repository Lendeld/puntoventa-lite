"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    ClienteDto,
    ClienteListaDto,
    ObtenerClientesPaginadoParams,
    SaldoCreditoClienteDto,
} from "@lib/types/clientes.types";

interface ClienteServiceParams {
    nombre: string;
    identificacion: string | null;
    correo: string | null;
    telefono: string | null;
    observaciones: string | null;
}

interface ActualizarClienteServiceParams extends ClienteServiceParams {
    id: string;
    activo: boolean;
}

export async function obtenerClientesService(
    params: ObtenerClientesPaginadoParams,
): Promise<DataAPI<PagedResult<ClienteListaDto>>> {
    return await requestAPI<PagedResult<ClienteListaDto>>({
        url: "/clientes",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            Activo: params.activo,
        },
    });
}

export async function obtenerClientePorIdService(id: string): Promise<DataAPI<ClienteDto>> {
    return await requestAPI<ClienteDto>({
        url: `/clientes/${id}`,
        method: "GET",
    });
}

export async function crearClienteService(body: ClienteServiceParams): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/clientes",
        method: "POST",
        body,
    });
}

export async function actualizarClienteService(
    body: ActualizarClienteServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/clientes/${body.id}`,
        method: "PUT",
        body,
    });
}

export async function obtenerSaldoCreditoClienteService(id: string): Promise<DataAPI<SaldoCreditoClienteDto>> {
    return await requestAPI<SaldoCreditoClienteDto>({
        url: `/clientes/${id}/saldo-credito`,
        method: "GET",
    });
}
