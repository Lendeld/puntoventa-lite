import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarUsuarioSchema,
    crearUsuarioSchema,
} from "@lib/schemas/usuarios.schema";

export interface UsuarioDto {
    id: string;
    nombreUsuario: string;
    nombre: string;
    identificacion: string;
    negocioId: string | null;
    rolId: string | null;
    rolNombre: string | null;
    esPropietario: boolean;
    correo: string | null;
    telefono: string | null;
    debeCambiarPassword?: boolean;
    activo: boolean;
    fechaCreacion: string;
    fechaModificacion: string | null;
    creadoPor: string | null;
    modificadoPor: string | null;
}

export interface ObtenerUsuariosPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    activo?: boolean;
}

export type CrearUsuarioFormValues = FormValues<typeof crearUsuarioSchema>;
export type ActualizarUsuarioFormValues = FormValues<typeof actualizarUsuarioSchema>;
