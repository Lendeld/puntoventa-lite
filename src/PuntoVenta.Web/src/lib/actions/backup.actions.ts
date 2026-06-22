"use server";

import {
    generarBackupService,
    obtenerVersionSistemaService,
    validarBackupService,
} from "@lib/services/backup.service";
import type { BackupGeneradoDto, BackupValidacionDto, VersionSistemaDto } from "@lib/types/backup.types";
import type { ActionResult } from "@lib/types/base.types";

export interface BackupActionResult<T = undefined> extends ActionResult {
    data: T | null;
}

export async function generarBackupAction(
    pin: string,
    rutaDestino: string,
): Promise<BackupActionResult<BackupGeneradoDto>> {
    if (!pin || !/^\d{6}$/.test(pin)) {
        return {
            status: 400,
            data: null,
            errors: { "Usuario_Pin": "El PIN debe ser exactamente 6 dígitos numéricos." },
        };
    }
    if (!rutaDestino || rutaDestino.trim().length === 0) {
        return {
            status: 400,
            data: null,
            errors: { "general": "Debe especificar una ruta de destino para el respaldo." },
        };
    }

    const response = await generarBackupService(pin, rutaDestino.trim());

    if (response.errors) {
        return {
            status: response.errors.status,
            data: null,
            errors: response.errors.errors,
        };
    }

    return { status: 200, data: response.data ?? null, errors: undefined };
}

export async function validarBackupAction(
    rutaBackup: string,
    pin: string,
): Promise<BackupActionResult<BackupValidacionDto>> {
    if (!pin || !/^\d{6}$/.test(pin)) {
        return {
            status: 400,
            data: null,
            errors: { "Usuario_Pin": "El PIN debe ser exactamente 6 dígitos numéricos." },
        };
    }
    if (!rutaBackup || rutaBackup.trim().length === 0) {
        return {
            status: 400,
            data: null,
            errors: { "general": "Debe especificar la ruta del archivo de respaldo." },
        };
    }

    const response = await validarBackupService(rutaBackup.trim(), pin);

    if (response.errors) {
        return {
            status: response.errors.status,
            data: null,
            errors: response.errors.errors,
        };
    }

    return { status: 200, data: response.data ?? null, errors: undefined };
}

export async function obtenerVersionSistemaAction(): Promise<BackupActionResult<VersionSistemaDto>> {
    const response = await obtenerVersionSistemaService();

    if (response.errors) {
        return {
            status: response.errors.status,
            data: null,
            errors: response.errors.errors,
        };
    }

    return { status: 200, data: response.data ?? null, errors: undefined };
}
