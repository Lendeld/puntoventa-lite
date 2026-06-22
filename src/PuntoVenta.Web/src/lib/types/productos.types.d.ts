import type { FormValues } from "@lib/types/base.types";
import type {
    crearProductoSchema,
    editarProductoSchema,
} from "@lib/schemas/productos.schema";

export const enum TipoItem {
    Bien = 1,
    Servicio = 2,
}

export interface ProductoDto {
    id: string;
    codigo: string;
    codigoBarras: string | null;
    nombre: string;
    descripcion: string | null;
    tipoItem: "Bien" | "Servicio";
    imagenUrl: string | null;
    precioUnitario: number;
    precioCosto: number | null;
    categoriaId: string | null;
    tarifaIvaImpuestoCodigo: string | null;
    noAplicaExistencias: boolean;
    permiteModificarPrecioUnitario: boolean;
    existenciaTotal: number;
    fechaCreacion: string;
    fechaModificacion: string | null;
}

export interface ObtenerProductosPaginadoParams {
    numeroPagina: number;
    tamanoPagina: number;
    filtroDinamico?: string;
    tipoItem?: number;
    categoriaId?: string;
}

export type CrearProductoFormValues = FormValues<typeof crearProductoSchema>;
export type EditarProductoFormValues = FormValues<typeof editarProductoSchema>;
