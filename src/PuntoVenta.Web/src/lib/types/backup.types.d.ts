export interface BackupGeneradoDto {
    rutaArchivo: string;
    versionEsquema: string;
    fechaUtc: string;
    appVersion: string | null;
}

export interface BackupValidacionDto {
    esCompatible: boolean;
    versionBackup: string;
    versionApp: string;
    // Token de capacidad de un solo uso; el bridge lo pasa a backup:restaurar y el
    // main lo consume contra el backend antes del swap. Vacío si no es compatible.
    tokenRestauracion: string;
}

// El endpoint GET /backup/version-sistema devuelve la versión del sistema (ej. "1.0.6")
// como string crudo (no un objeto), por eso el tipo es un alias de string.
export type VersionSistemaDto = string;
