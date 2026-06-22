"use client";

import { AppNotifier } from "@components/ui/AppNotifier";
import { getBridge } from "@lib/printing/electron-bridge";
import { getAbonoPdfUrl, getVentaPdfUrl } from "@lib/printing/venta-printing";
import { obtenerPerfilesImpresoraService } from "@lib/services/impresion.service";
import { obtenerTicketDataService } from "@lib/services/impresion.service";
import type { PerfilImpresoraTicketDto } from "@lib/types/impresion.types";
import type { ConfigImpresionLocal } from "@lib/types/impresion-bridge";

export type ImprimirTicketResult =
    | { status: "ok" }
    | { status: "skipped"; reason: string }
    | { status: "error"; message: string };

// ---------------------------------------------------------------------------
// Perfil cache — short-lived in-memory store so repeated calls in the same
// session avoid extra server requests.
// ---------------------------------------------------------------------------
let _perfilesCache: PerfilImpresoraTicketDto[] | null = null;
let _perfilesCacheTs = 0;
const PERFILES_CACHE_TTL_MS = 60_000;

async function obtenerPerfiles(): Promise<PerfilImpresoraTicketDto[]> {
    const now = Date.now();
    if (_perfilesCache && now - _perfilesCacheTs < PERFILES_CACHE_TTL_MS) {
        return _perfilesCache;
    }
    const res = await obtenerPerfilesImpresoraService();
    if (res.errors || !res.data) return [];
    _perfilesCache = res.data;
    _perfilesCacheTs = now;
    return _perfilesCache;
}

/** Invalidates the in-memory perfiles cache (used in tests). */
export function _invalidarCachePerfiles(): void {
    _perfilesCache = null;
    _perfilesCacheTs = 0;
}

// ---------------------------------------------------------------------------
// Core printing logic
// ---------------------------------------------------------------------------

async function imprimirConBridge(
    documentoId: string,
    config: ConfigImpresionLocal,
    perfiles: PerfilImpresoraTicketDto[],
    pagoId?: string,
): Promise<ImprimirTicketResult> {
    const bridge = getBridge();
    if (!bridge) {
        return { status: "skipped", reason: "bridge-unavailable" };
    }

    if (!config.impresora) {
        return {
            status: "error",
            message:
                "No hay impresora configurada. Configurala en Sistema › Mi Negocio › Ticket.",
        };
    }

    const perfil = config.perfilClave
        ? perfiles.find((p) => p.clave === config.perfilClave)
        : undefined;

    if (!perfil) {
        return {
            status: "error",
            message:
                "No se encontró el perfil de impresora configurado. Verificá la configuración.",
        };
    }

    const ticketRes = await obtenerTicketDataService(documentoId, pagoId);
    if (ticketRes.errors || !ticketRes.data) {
        return {
            status: "error",
            message: "No se pudo obtener los datos del ticket desde el servidor.",
        };
    }

    const resultado = await bridge.imprimirTicket({
        impresora: config.impresora,
        perfil,
        ticket: ticketRes.data,
        abrirGaveta: config.abrirGavetaAlCobrar,
        copias: config.copias,
    });

    if (!resultado.ok) {
        return {
            status: "error",
            message: resultado.error ?? "Error desconocido al imprimir.",
        };
    }

    return { status: "ok" };
}

/**
 * Attempts to print a ticket for the given document.
 *
 * - If the Electron bridge is present: reads the local config + matching
 *   printer profile, fetches ticket data from the backend, then calls
 *   `window.pulpoImpresion.imprimirTicket`.
 * - If there is no bridge (normal browser): opens the PDF fallback in a new
 *   tab so the user can print via the browser.
 */
export async function imprimirTicketAhora(
    documentoId: string,
    pagoId?: string,
): Promise<ImprimirTicketResult> {
    const bridge = getBridge();

    if (!bridge) {
        // Browser fallback — open the PDF so the user can print.
        const url = pagoId
            ? getAbonoPdfUrl(documentoId, pagoId)
            : getVentaPdfUrl(documentoId);
        window.open(url, "_blank", "noopener,noreferrer");
        return { status: "skipped", reason: "no-bridge-pdf-fallback" };
    }

    const [config, perfiles] = await Promise.all([
        bridge.obtenerConfig(),
        obtenerPerfiles(),
    ]);

    const cfg: ConfigImpresionLocal = config ?? {
        impresora: null,
        perfilClave: null,
        abrirGavetaAlCobrar: false,
        copias: 1,
    };

    return imprimirConBridge(documentoId, cfg, perfiles, pagoId);
}

/**
 * Same as `imprimirTicketAhora` but only runs when the bridge is present AND
 * the user has enabled auto-print (`abrirGavetaAlCobrar` is intentionally
 * separate from auto-print, but the config has `copias >= 1` to enable the
 * feature).  If there is no bridge this returns "skipped" silently.
 *
 * In the current model, auto-print runs when `config.impresora` and
 * `config.perfilClave` are both set — that is the user's opt-in signal.
 */
export async function imprimirTicketAuto(
    documentoId: string,
): Promise<ImprimirTicketResult> {
    const bridge = getBridge();
    if (!bridge) return { status: "skipped", reason: "no-bridge" };

    const config = await bridge.obtenerConfig();
    if (!config || !config.impresora || !config.perfilClave) {
        return { status: "skipped", reason: "not-configured" };
    }

    const perfiles = await obtenerPerfiles();
    return imprimirConBridge(documentoId, config, perfiles);
}

/**
 * Hook-like helper for components: calls `imprimirTicketAhora` and shows an
 * AppNotifier on error.  Returns a stable function — callers do NOT need
 * useCallback because this module-level factory is recreated only once.
 */
export function useImprimirTicketAhora() {
    return async (documentoId: string, pagoId?: string): Promise<void> => {
        const result = await imprimirTicketAhora(documentoId, pagoId);
        if (result.status === "error") {
            AppNotifier.error({
                title: "No se pudo imprimir el ticket",
                message: result.message,
            });
        }
    };
}
