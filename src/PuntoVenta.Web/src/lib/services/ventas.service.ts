"use server";

import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI, PagedResult } from "@lib/types/base.types";
import type {
    AbonarApartadoPayload,
    CrearBorradorFacturaPayload,
    CrearProformaPayload,
    DocumentoVentaDto,
    DocumentoVentaResumenDto,
    EmitirNotaCreditoPayload,
    EmitirNotaDebitoPayload,
    FacturarProformaPayload,
    ExtenderVencimientoApartadoPayload,
    ObtenerDocumentosVentaPaginadoParams,
    VentasCatalogosDto,
    FacturaCreditoResumenDto,
    ObtenerFacturasCreditoParams,
    RegistrarAbonoFacturaRequest,
    ReporteMovimientosDineroResultado,
    DocumentoVentaEventoListaDto,
    ObtenerReporteMovimientosDineroParams,
    ObtenerReporteVentasRangoParams,
    ReporteVentasRangoResultado,
} from "@lib/types/ventas.types";

export async function crearFacturaService(
    body: CrearBorradorFacturaPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/ventas/facturas",
        method: "POST",
        body,
    });
}

export async function crearProformaService(
    body: CrearProformaPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/ventas/proformas",
        method: "POST",
        body,
    });
}

export async function crearApartadoService(
    body: CrearBorradorFacturaPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/ventas/apartados",
        method: "POST",
        body,
    });
}

export async function abonarApartadoService(
    id: string,
    body: AbonarApartadoPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/apartados/${id}/abonos`,
        method: "POST",
        body,
    });
}

export async function extenderVencimientoApartadoService(
    id: string,
    body: ExtenderVencimientoApartadoPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/apartados/${id}/extender-vencimiento`,
        method: "POST",
        body,
    });
}

export async function convertirApartadoAFacturaService(id: string): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/apartados/${id}/convertir`,
        method: "POST",
    });
}

export async function cancelarApartadoService(id: string): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/apartados/${id}/cancelar`,
        method: "POST",
    });
}

export async function actualizarProformaService(
    id: string,
    body: CrearProformaPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/proformas/${id}`,
        method: "PUT",
        body,
    });
}

export async function facturarProformaService(
    id: string,
    body: FacturarProformaPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/proformas/${id}/facturar`,
        method: "POST",
        body,
    });
}

export async function obtenerDocumentoVentaPorIdService(id: string): Promise<DataAPI<DocumentoVentaDto>> {
    return await requestAPI<DocumentoVentaDto>({
        url: `/ventas/${id}`,
        method: "GET",
    });
}

export async function obtenerDocumentosVentaService(
    params: ObtenerDocumentosVentaPaginadoParams,
): Promise<DataAPI<PagedResult<DocumentoVentaResumenDto>>> {
    return await requestAPI<PagedResult<DocumentoVentaResumenDto>>({
        url: "/ventas",
        method: "GET",
        query: {
            NumeroPagina: params.numeroPagina,
            TamanoPagina: params.tamanoPagina,
            FiltroDinamico: params.filtroDinamico,
            TipoDocumento: params.tipoDocumento,
            Estado: params.estado,
            ClienteId: params.clienteId,
            FechaDesde: params.fechaDesde,
            FechaHasta: params.fechaHasta,
        },
    });
}

export async function obtenerCatalogosVentasService(): Promise<DataAPI<VentasCatalogosDto>> {
    return await requestAPI<VentasCatalogosDto>({
        url: "/ventas/catalogos",
        method: "GET",
    });
}

export async function obtenerFacturasCreditoService(
    params: ObtenerFacturasCreditoParams,
): Promise<DataAPI<PagedResult<FacturaCreditoResumenDto>>> {
    return await requestAPI<PagedResult<FacturaCreditoResumenDto>>({
        url: "/ventas/credito",
        method: "GET",
        query: {
            Pagina: params.pagina,
            Tamano: params.tamano,
            Filtro: params.filtro,
            ClienteId: params.clienteId,
            SoloVencidas: params.soloVencidas,
        },
    });
}

export async function emitirNotaCreditoService(
    body: EmitirNotaCreditoPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/ventas/notas-credito",
        method: "POST",
        body,
    });
}

export async function emitirNotaDebitoService(
    body: EmitirNotaDebitoPayload,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: "/ventas/notas-debito",
        method: "POST",
        body,
    });
}

export async function registrarAbonoFacturaService(
    id: string,
    body: RegistrarAbonoFacturaRequest,
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/facturas/${id}/abonos`,
        method: "POST",
        body,
    });
}

export async function anularAbonoFacturaService(
    documentoId: string,
    pagoId: string,
    body: { motivo: string },
): Promise<DataAPI<string>> {
    return await requestAPI<string>({
        url: `/ventas/${documentoId}/abonos/${pagoId}/anular`,
        method: "POST",
        body,
    });
}

export async function obtenerEventosVentaService(
    id: string,
    params: { skip?: number; take?: number } = {},
): Promise<DataAPI<DocumentoVentaEventoListaDto>> {
    return await requestAPI<DocumentoVentaEventoListaDto>({
        url: `/ventas/${id}/eventos`,
        method: "GET",
        query: {
            Skip: params.skip ?? 0,
            Take: params.take ?? 50,
        },
    });
}

export async function obtenerReporteVentasRangoService(
    params: ObtenerReporteVentasRangoParams,
): Promise<DataAPI<ReporteVentasRangoResultado>> {
    return await requestAPI<ReporteVentasRangoResultado>({
        url: "/ventas/reportes/rango",
        method: "GET",
        query: {
            FechaDesde: params.fechaDesde,
            FechaHasta: params.fechaHasta,
            Consecutivo: params.consecutivo,
            Colonizar: params.colonizar,
            Detallado: params.detallado,
        },
    });
}

export async function obtenerReporteMovimientosDineroService(
    params: ObtenerReporteMovimientosDineroParams,
): Promise<DataAPI<ReporteMovimientosDineroResultado>> {
    return await requestAPI<ReporteMovimientosDineroResultado>({
        url: "/ventas/reportes/movimientos-dinero",
        method: "GET",
        query: {
            FechaDesde: params.fechaDesde,
            FechaHasta: params.fechaHasta,
            CajaId: params.cajaId,
        },
    });
}
