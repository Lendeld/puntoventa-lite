import type { FormValues } from "@lib/types/base.types";
import type { actualizarVendedorSchema, crearVendedorSchema } from "@lib/schemas/vendedores.schema";

export interface VendedorDto {
    id: string;
    nombre: string;
    isPrincipal: boolean;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface VendedorActivoDto {
    id: string;
    nombre: string;
    isPrincipal: boolean;
}

export interface ObtenerVendedoresPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export type CrearVendedorFormValues = FormValues<typeof crearVendedorSchema>;
export type ActualizarVendedorFormValues = FormValues<typeof actualizarVendedorSchema>;
