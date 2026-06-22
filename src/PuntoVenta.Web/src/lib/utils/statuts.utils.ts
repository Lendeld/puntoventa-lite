import { EstadoFiltro } from "@lib/types/base.types";

export function statusSegmentToActivo(value: EstadoFiltro): boolean | undefined {
    if (value === "activos") return true;
    if (value === "inactivos") return false;
    return undefined;
}
