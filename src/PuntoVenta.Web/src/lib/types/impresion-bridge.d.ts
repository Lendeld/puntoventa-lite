import type { PerfilImpresoraTicketDto } from "@lib/types/impresion.types";
import type { TicketDataDto } from "@lib/types/ventas.types";

export type ResultadoImpresion = { ok: boolean; error?: string };
export type ConfigImpresionLocal = {
    impresora: string | null;
    perfilClave: string | null;
    abrirGavetaAlCobrar: boolean;
    copias: number;
};
export type ImpresoraInfo = {
    nombre: string;
    origen: "sistema" | "serial";
    esDefault: boolean;
};

export interface PulpoImpresion {
    listarImpresoras(): Promise<ImpresoraInfo[]>;
    imprimirTicket(req: {
        impresora: string;
        perfil: PerfilImpresoraTicketDto;
        ticket: TicketDataDto;
        abrirGaveta: boolean;
        copias: number;
    }): Promise<ResultadoImpresion>;
    imprimirPrueba(req: {
        impresora: string;
        perfil: PerfilImpresoraTicketDto;
    }): Promise<ResultadoImpresion>;
    abrirGaveta(req: {
        impresora: string;
        perfil: PerfilImpresoraTicketDto;
    }): Promise<ResultadoImpresion>;
    obtenerConfig(): Promise<ConfigImpresionLocal | null>;
    guardarConfig(cfg: ConfigImpresionLocal): Promise<{ ok: boolean }>;
    imprimirHtml(req: {
        html: string;
        impresora?: string;
        anchoMm: number;
    }): Promise<ResultadoImpresion>;
}

declare global {
    interface Window {
        pulpoImpresion?: PulpoImpresion;
    }
}
