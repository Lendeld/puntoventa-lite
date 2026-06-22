import type { FormValues } from "@lib/types/base.types";
import type { actualizarNegocioSchema } from "@lib/schemas/configuracion.schema";

export interface NegocioDto {
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
    logoUrl: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export interface TipoIdentificacionDto {
    id: string;
    codigo: string;
    detalle: string;
    comentario: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export interface CondicionVentaDto {
    id: string;
    codigo: string;
    detalle: string;
    comentario: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export interface MedioPagoDto {
    id: string;
    codigo: string;
    detalle: string;
    comentario: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export interface CodigoImpuestoDto {
    id: string;
    codigo: string;
    detalle: string;
    comentario: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export interface TarifaIvaImpuestoDto {
    id: string;
    codigo: string;
    detalle: string;
    porcentaje: number;
    comentario: string | null;
    activo: boolean;
    modificadoPor: string | null;
    fechaModificacion: string | null;
}

export type ActualizarNegocioFormValues = FormValues<typeof actualizarNegocioSchema>;
export type CrearNegocioFormValues = FormValues<typeof actualizarNegocioSchema>;
