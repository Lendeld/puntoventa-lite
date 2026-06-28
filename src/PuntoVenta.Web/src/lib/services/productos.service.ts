"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type { ObtenerProductosPaginadoParams, ProductoDto } from "@lib/types/productos.types";

interface CrearProductoServiceParams {
    codigo: string;
    nombre: string;
    tipoItem: number;
    precioUnitario: number | null | undefined;
    codigoBarras: string | null;
    descripcion: string | null;
    precioCosto: number | null;
    categoriaId: string | null;
    proveedorId?: string | null;
    tarifaIvaImpuestoCodigo: string | null;
    noAplicaExistencias?: boolean;
    permiteModificarPrecioUnitario?: boolean;
    existenciaInicial?: number | null;
}

interface ActualizarProductoServiceParams extends CrearProductoServiceParams {
    id: string;
}

export async function obtenerProductosService(
    params: ObtenerProductosPaginadoParams,
): Promise<DataAPI<PagedResult<ProductoDto>>> {
    return await requestAPI<PagedResult<ProductoDto>>({
        url: "/productos",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            TipoItem: params.tipoItem,
            CategoriaId: params.categoriaId,
        },
    });
}

export async function obtenerProductoPorIdService(id: string): Promise<DataAPI<ProductoDto>> {
    return await requestAPI<ProductoDto>({
        url: `/productos/${id}`,
        method: "GET",
    });
}

export async function obtenerProductoPorCodigoBarrasService(
    codigoBarras: string,
): Promise<DataAPI<ProductoDto>> {
    return await requestAPI<ProductoDto>({
        url: `/productos/codigo-barras/${encodeURIComponent(codigoBarras)}`,
        method: "GET",
    });
}

export async function crearProductoService(
    body: CrearProductoServiceParams,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/productos",
        method: "POST",
        body,
    });
}

export async function actualizarProductoService(
    body: ActualizarProductoServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/productos/${body.id}`,
        method: "PUT",
        body: {
            codigo: body.codigo,
            nombre: body.nombre,
            tipoItem: body.tipoItem,
            precioUnitario: body.precioUnitario,
            codigoBarras: body.codigoBarras,
            descripcion: body.descripcion,
            precioCosto: body.precioCosto,
            categoriaId: body.categoriaId,
            proveedorId: body.proveedorId,
            tarifaIvaImpuestoCodigo: body.tarifaIvaImpuestoCodigo,
            noAplicaExistencias: body.noAplicaExistencias,
            permiteModificarPrecioUnitario: body.permiteModificarPrecioUnitario,
        },
    });
}

export async function toggleEstadoProductoService(id: string): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({
        url: `/productos/${id}/estado`,
        method: "PATCH",
    });
}
