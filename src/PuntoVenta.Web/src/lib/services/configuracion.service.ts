"use server";

import { requestAPI } from "@lib/utils/requestApi";
import apiClient from "@lib/utils/apiClient";
import type { DataAPI, ProblemDetail } from "@lib/types/base.types";
import type {
    NegocioDto,
    TipoIdentificacionDto,
    CondicionVentaDto,
    MedioPagoDto,
    CodigoImpuestoDto,
    TarifaIvaImpuestoDto,
} from "@lib/types/configuracion.types";
import type { AuthFlowResponse } from "@lib/types/auth.types";

interface ActualizarNegocioServiceParams {
    id: string;
    nombre: string;
    nombreComercial: string | null;
    direccion: string;
    tipoIdentificacionCodigo: string;
    identificacion: string;
    correo: string;
    telefono: string | null;
    aplicaVendedores: boolean;
    aplicaCajas: boolean;
    tipoCambioPredeterminado: number;
}

interface CrearNegocioServiceParams {
    nombre: string;
    nombreComercial: string | null;
    direccion: string;
    tipoIdentificacionCodigo: string;
    identificacion: string;
    correo: string;
    telefono: string | null;
    aplicaVendedores: boolean;
}

export async function obtenerNegocioService(): Promise<DataAPI<NegocioDto>> {
    return await requestAPI<NegocioDto>({
        url: "/negocio",
        method: "GET",
    });
}

export async function actualizarNegocioService(
    body: ActualizarNegocioServiceParams,
): Promise<DataAPI<null>> {
    return await requestAPI<null>({
        url: `/negocio/${body.id}`,
        method: "PUT",
        body: {
            nombre: body.nombre,
            nombreComercial: body.nombreComercial,
            direccion: body.direccion,
            tipoIdentificacionCodigo: body.tipoIdentificacionCodigo,
            identificacion: body.identificacion,
            correo: body.correo,
            telefono: body.telefono,
            aplicaVendedores: body.aplicaVendedores,
            aplicaCajas: body.aplicaCajas,
            tipoCambioPredeterminado: body.tipoCambioPredeterminado,
        },
    });
}

export async function subirLogoNegocioService(
    archivo: File,
): Promise<DataAPI<string>> {
    const formData = new FormData();
    formData.append("logo", archivo);

    try {
        const response = await apiClient.postFormData<string>("/negocio/logo", formData);
        if (response.status === 200) {
            return { data: response.data, errors: null };
        }
        return { data: null, errors: response.data as unknown as ProblemDetail };
    } catch {
        return { data: null, errors: { title: "Error al subir logo", status: 500 } as ProblemDetail };
    }
}

export async function crearNegocioService(
    body: CrearNegocioServiceParams,
): Promise<DataAPI<AuthFlowResponse>> {
    return await requestAPI<AuthFlowResponse>({
        url: "/negocio",
        method: "POST",
        body: {
            nombre: body.nombre,
            nombreComercial: body.nombreComercial,
            direccion: body.direccion,
            tipoIdentificacionCodigo: body.tipoIdentificacionCodigo,
            identificacion: body.identificacion,
            correo: body.correo,
            telefono: body.telefono,
            aplicaVendedores: body.aplicaVendedores,
        },
    });
}

export async function obtenerTiposIdentificacionService(
    activo?: boolean,
): Promise<DataAPI<TipoIdentificacionDto[]>> {
    return await requestAPI<TipoIdentificacionDto[]>({
        url: "/tipos-identificacion",
        method: "GET",
        query: {
            Activo: activo,
        },
    });
}

export async function toggleEstadoTipoIdentificacionService(
    id: string,
): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({
        url: `/tipos-identificacion/${id}/estado`,
        method: "PATCH",
    });
}

export async function obtenerCondicionesVentaService(
    activo?: boolean,
): Promise<DataAPI<CondicionVentaDto[]>> {
    return await requestAPI<CondicionVentaDto[]>({
        url: "/condiciones-venta",
        method: "GET",
        query: { Activo: activo },
    });
}

export async function toggleEstadoCondicionVentaService(id: string): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({ url: `/condiciones-venta/${id}/estado`, method: "PATCH" });
}

export async function obtenerMediosPagoService(
    activo?: boolean,
): Promise<DataAPI<MedioPagoDto[]>> {
    return await requestAPI<MedioPagoDto[]>({
        url: "/medios-pago",
        method: "GET",
        query: { Activo: activo },
    });
}

export async function toggleEstadoMedioPagoService(id: string): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({ url: `/medios-pago/${id}/estado`, method: "PATCH" });
}

export async function obtenerCodigosImpuestoService(
    activo?: boolean,
): Promise<DataAPI<CodigoImpuestoDto[]>> {
    return await requestAPI<CodigoImpuestoDto[]>({
        url: "/codigos-impuesto",
        method: "GET",
        query: { Activo: activo },
    });
}

export async function toggleEstadoCodigoImpuestoService(id: string): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({ url: `/codigos-impuesto/${id}/estado`, method: "PATCH" });
}

export async function obtenerTarifasIvaService(
    activo?: boolean,
): Promise<DataAPI<TarifaIvaImpuestoDto[]>> {
    return await requestAPI<TarifaIvaImpuestoDto[]>({
        url: "/tarifas-iva",
        method: "GET",
        query: { Activo: activo },
    });
}

export async function toggleEstadoTarifaIvaService(id: string): Promise<DataAPI<boolean>> {
    return await requestAPI<boolean>({ url: `/tarifas-iva/${id}/estado`, method: "PATCH" });
}
