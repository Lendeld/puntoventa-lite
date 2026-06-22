"use server"
import { validarPermisoService } from "@lib/services/auth.service";

export async function tienePermiso(permiso: string): Promise<boolean> {
    if (!permiso.trim()) return false;

    const response = await validarPermisoService(permiso);
    return !response.errors;
}
