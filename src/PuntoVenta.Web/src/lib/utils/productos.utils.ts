import type { ProductoDto } from "@lib/types/productos.types";

const STOCK_BAJO_UMBRAL = 5;

export type EstadoStock = "sin-control" | "agotado" | "bajo" | "disponible";

export function obtenerEstadoStock(producto: {
    noAplicaExistencias: boolean;
    existenciaTotal: number;
    tipoItem?: ProductoDto["tipoItem"];
}): EstadoStock {
    // Un servicio no maneja existencias: nunca se muestra como agotado.
    if (producto.tipoItem === "Servicio") return "sin-control";
    if (producto.noAplicaExistencias) return "sin-control";
    if (producto.existenciaTotal <= 0) return "agotado";
    if (producto.existenciaTotal <= STOCK_BAJO_UMBRAL) return "bajo";
    return "disponible";
}

// Color Mantine por estado de stock. Acompaña siempre texto (color-not-only).
export function colorPorEstadoStock(estado: EstadoStock): string {
    switch (estado) {
        case "agotado":
            return "red";
        case "bajo":
            return "yellow";
        case "disponible":
            return "teal";
        case "sin-control":
            return "gray";
    }
}

// Etiqueta corta del badge de stock para el dropdown de productos.
export function etiquetaStock(producto: {
    noAplicaExistencias: boolean;
    existenciaTotal: number;
    tipoItem?: ProductoDto["tipoItem"];
}): string | null {
    const estado = obtenerEstadoStock(producto);
    switch (estado) {
        case "sin-control":
            return null;
        case "agotado":
            return "Agotado";
        case "bajo":
            return `Quedan ${producto.existenciaTotal}`;
        case "disponible":
            return `${producto.existenciaTotal} en stock`;
    }
}

// Color Mantine unificado para el tipo de ítem (Bien/Servicio).
export function colorPorTipoItem(tipoItem: ProductoDto["tipoItem"]): string {
    return tipoItem === "Bien" ? "blue" : "grape";
}
