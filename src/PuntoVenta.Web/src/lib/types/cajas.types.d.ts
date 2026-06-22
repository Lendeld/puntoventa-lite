import type { FormValues } from "@lib/types/base.types";
import type {
    actualizarCajaSchema,
    crearCajaSchema,
} from "@lib/schemas/cajas.schema";

export interface CajaListadoItemDto {
    id: string;
    codigo: string;
    nombre: string;
    activo: boolean;
}

export type CrearCajaFormValues = FormValues<typeof crearCajaSchema>;
export type ActualizarCajaFormValues = FormValues<typeof actualizarCajaSchema>;
