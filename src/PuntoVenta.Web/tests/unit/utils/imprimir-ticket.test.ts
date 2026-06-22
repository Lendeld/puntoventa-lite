import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import type { PulpoImpresion } from "@lib/types/impresion-bridge";
import type { PerfilImpresoraTicketDto } from "@lib/types/impresion.types";
import type { TicketDataDto } from "@lib/types/ventas.types";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeMinimalPerfil(clave: string): PerfilImpresoraTicketDto {
    return {
        id: "perfil-1",
        clave,
        nombre: "Perfil de prueba",
        anchoMm: 80,
        charsPorLinea: 42,
        codepage: "PC850",
        drawerPin: 2,
        comandoCorte: "PartialCut",
        densidad: 0,
        activo: true,
    };
}

// A minimal TicketDataDto shape sufficient for the tests.
const fakeTicketData = { documentoId: "doc-1" } as unknown as TicketDataDto;

function makeBridge(overrides: Partial<PulpoImpresion> = {}): PulpoImpresion {
    return {
        listarImpresoras: vi.fn().mockResolvedValue([]),
        imprimirTicket: vi.fn().mockResolvedValue({ ok: true }),
        imprimirPrueba: vi.fn().mockResolvedValue({ ok: true }),
        abrirGaveta: vi.fn().mockResolvedValue({ ok: true }),
        obtenerConfig: vi.fn().mockResolvedValue({
            impresora: "HP LaserJet",
            perfilClave: "80mm",
            abrirGavetaAlCobrar: false,
            copias: 1,
        }),
        guardarConfig: vi.fn().mockResolvedValue({ ok: true }),
        imprimirHtml: vi.fn().mockResolvedValue({ ok: true }),
        ...overrides,
    };
}

function setWindowBridge(value: unknown): void {
    Object.defineProperty(window, "pulpoImpresion", {
        value,
        writable: true,
        configurable: true,
    });
}

function removeWindowBridge(): void {
    if ("pulpoImpresion" in window) {
        // eslint-disable-next-line @typescript-eslint/no-dynamic-delete
        delete (window as unknown as Record<string, unknown>)["pulpoImpresion"];
    }
}

// ---------------------------------------------------------------------------
// Mock the server service so tests don't hit the real server action
// ---------------------------------------------------------------------------
vi.mock("@lib/services/impresion.service", () => ({
    obtenerTicketDataService: vi.fn().mockResolvedValue({ data: fakeTicketData, errors: undefined }),
    obtenerPerfilesImpresoraService: vi.fn().mockResolvedValue({
        data: [makeMinimalPerfil("80mm")],
        errors: undefined,
    }),
    // Other exports from the module that aren't under test
    obtenerNegocioTicketConfigService: vi.fn(),
    actualizarNegocioTicketConfigService: vi.fn(),
}));

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("imprimirTicketAhora", () => {
    afterEach(() => {
        removeWindowBridge();
        vi.clearAllMocks();
    });

    it("falls back to PDF open when bridge is absent (browser mode)", async () => {
        removeWindowBridge();
        const openSpy = vi.spyOn(window, "open").mockImplementation(() => null);

        // Dynamically import AFTER bridge is absent so the module reads window state
        // We need to bypass the module cache, so we do it via a dynamic import trick
        // re-using the existing module (already cached).
        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-abc");

        expect(result.status).toBe("skipped");
        expect(openSpy).toHaveBeenCalledWith(
            "/pdf/ventas/doc-abc",
            "_blank",
            "noopener,noreferrer",
        );

        openSpy.mockRestore();
    });

    it("falls back to abono PDF when pagoId is provided and no bridge exists", async () => {
        removeWindowBridge();
        const openSpy = vi.spyOn(window, "open").mockImplementation(() => null);

        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-abc", "pago-1");

        expect(result.status).toBe("skipped");
        expect(openSpy).toHaveBeenCalledWith(
            "/pdf/ventas/doc-abc/abonos/pago-1",
            "_blank",
            "noopener,noreferrer",
        );

        openSpy.mockRestore();
    });

    it("calls bridge.imprimirTicket when bridge is present and config is complete", async () => {
        const bridge = makeBridge();
        setWindowBridge(bridge);

        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-1");

        expect(result.status).toBe("ok");
        expect(bridge.imprimirTicket).toHaveBeenCalledWith(
            expect.objectContaining({
                impresora: "HP LaserJet",
                ticket: fakeTicketData,
                abrirGaveta: false,
                copias: 1,
            }),
        );
    });

    it("returns error when bridge.imprimirTicket returns ok=false", async () => {
        const bridge = makeBridge({
            imprimirTicket: vi.fn().mockResolvedValue({
                ok: false,
                error: "Papel atascado",
            }),
        });
        setWindowBridge(bridge);

        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-1");

        expect(result.status).toBe("error");
        if (result.status === "error") {
            expect(result.message).toContain("Papel atascado");
        }
    });

    it("returns error when config has no impresora", async () => {
        const bridge = makeBridge({
            obtenerConfig: vi.fn().mockResolvedValue({
                impresora: null,
                perfilClave: "80mm",
                abrirGavetaAlCobrar: false,
                copias: 1,
            }),
        });
        setWindowBridge(bridge);

        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-1");

        expect(result.status).toBe("error");
    });

    it("returns error when perfilClave does not match any perfil", async () => {
        const bridge = makeBridge({
            obtenerConfig: vi.fn().mockResolvedValue({
                impresora: "HP",
                perfilClave: "nonexistent-profile",
                abrirGavetaAlCobrar: false,
                copias: 1,
            }),
        });
        setWindowBridge(bridge);

        const { imprimirTicketAhora, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAhora("doc-1");

        expect(result.status).toBe("error");
    });
});

describe("imprimirTicketAuto", () => {
    beforeEach(() => {
        removeWindowBridge();
        vi.clearAllMocks();
    });

    afterEach(() => {
        removeWindowBridge();
    });

    it("returns skipped with reason 'no-bridge' when bridge is absent", async () => {
        removeWindowBridge();
        const { imprimirTicketAuto, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAuto("doc-1");

        expect(result.status).toBe("skipped");
        if (result.status === "skipped") {
            expect(result.reason).toBe("no-bridge");
        }
    });

    it("returns skipped with reason 'not-configured' when config is empty", async () => {
        const bridge = makeBridge({
            obtenerConfig: vi.fn().mockResolvedValue({
                impresora: null,
                perfilClave: null,
                abrirGavetaAlCobrar: false,
                copias: 1,
            }),
        });
        setWindowBridge(bridge);

        const { imprimirTicketAuto, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAuto("doc-1");

        expect(result.status).toBe("skipped");
        if (result.status === "skipped") {
            expect(result.reason).toBe("not-configured");
        }
    });

    it("prints when bridge and config are both complete", async () => {
        const bridge = makeBridge();
        setWindowBridge(bridge);

        const { imprimirTicketAuto, _invalidarCachePerfiles } = await import("@lib/printing/imprimir-ticket");
        _invalidarCachePerfiles();

        const result = await imprimirTicketAuto("doc-1");

        expect(result.status).toBe("ok");
        expect(bridge.imprimirTicket).toHaveBeenCalledOnce();
    });
});
