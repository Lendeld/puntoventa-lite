import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarCategoriaSchema,
    crearCategoriaSchema,
} from "@lib/schemas/categorias.schema";

export interface CategoriaDto {
    id: string;
    nombre: string;
    descripcion: string | null;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface ObtenerCategoriasPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export type CrearCategoriaFormValues = FormValues<typeof crearCategoriaSchema>;
export type ActualizarCategoriaFormValues = FormValues<typeof actualizarCategoriaSchema>;
