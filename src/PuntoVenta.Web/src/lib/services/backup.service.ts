"use server";

import type {
    BackupGeneradoDto,
    BackupValidacionDto,
    VersionSistemaDto,
} from "@lib/types/backup.types";
import { requestAPI } from "@lib/utils/requestApi";
import type { DataAPI } from "@lib/types/base.types";

export async function generarBackupService(
    pin: string,
    rutaDestino: string,
): Promise<DataAPI<BackupGeneradoDto>> {
    return await requestAPI<BackupGeneradoDto>({
        url: "/backup/generar",
        method: "POST",
        body: { pin, rutaDestino },
    });
}

export async function validarBackupService(
    rutaBackup: string,
    pin: string,
): Promise<DataAPI<BackupValidacionDto>> {
    return await requestAPI<BackupValidacionDto>({
        url: "/backup/validar",
        method: "POST",
        body: { rutaBackup, pin },
    });
}

export async function obtenerVersionSistemaService(): Promise<DataAPI<VersionSistemaDto>> {
    return await requestAPI<VersionSistemaDto>({
        url: "/backup/version-sistema",
        method: "GET",
    });
}
