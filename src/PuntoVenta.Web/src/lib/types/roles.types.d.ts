import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarRolSchema,
    crearRolSchema,
} from "@lib/schemas/roles.schema";

export interface RolDto {
    id: string;
    nombre: string;
    descripcion: string | null;
    isPrincipal: boolean;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface ObtenerRolesPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export interface PaginaPermisosRolTabDto {
    paginaId: string;
    nombre: string;
    orden: number;
}

export interface PermisoPaginaDto {
    permisoId: string;
    clave: string;
    descripcion: string;
    asignado: boolean;
}

export interface PermisosRolPorPaginaDto {
    paginaId: string;
    nombre: string;
    permisos: PermisoPaginaDto[];
}

export type CrearRolFormValues = FormValues<typeof crearRolSchema>;
export type ActualizarRolFormValues = FormValues<typeof actualizarRolSchema>;
