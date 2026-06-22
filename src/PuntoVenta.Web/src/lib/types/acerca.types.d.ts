export interface AcercaDto {
    backendVersion: string;
    backendCommitSha: string | null;
    modoDespliegue: "Cloud" | "LocalHost";
    ambiente: string;
}
