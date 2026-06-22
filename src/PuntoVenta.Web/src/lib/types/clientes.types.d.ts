import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarClienteSchema,
    crearClienteSchema,
} from "@lib/schemas/clientes.schema";

export interface ClienteListaDto {
    id: string;
    nombre: string;
    identificacion: string | null;
    correo: string | null;
    telefono: string | null;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface ClienteDto extends ClienteListaDto {
    observaciones: string | null;
}

export interface SaldoCreditoClienteDto {
    clienteId: string;
    saldoVigente: number;
    saldoVencido: number;
    esMoroso: boolean;
    facturasVencidas: number;
    diasAtrasoMax: number;
}

export interface ObtenerClientesPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export type CrearClienteFormValues = FormValues<typeof crearClienteSchema>;
export type ActualizarClienteFormValues = FormValues<typeof actualizarClienteSchema>;
