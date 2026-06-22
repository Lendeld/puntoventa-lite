export interface EstadoTerminos {
    aceptado: boolean;
    versionVigente: string;
    versionAceptada: string | null;
    fechaAceptacionUtc: string | null;
}
