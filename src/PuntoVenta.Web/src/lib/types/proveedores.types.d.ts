import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarProveedorSchema,
    crearProveedorSchema,
} from "@lib/schemas/proveedores.schema";

export interface ProveedorDto {
    id: string;
    nombre: string;
    correo: string | null;
    telefono: string | null;
    observacion: string | null;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface ObtenerProveedoresPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export type CrearProveedorFormValues = FormValues<typeof crearProveedorSchema>;
export type ActualizarProveedorFormValues = FormValues<typeof actualizarProveedorSchema>;
