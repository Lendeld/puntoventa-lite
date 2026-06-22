import { describe, it, expect } from "vitest";
import {
    INIT_PRINTER,
    ALIGN_CENTER,
    ALIGN_LEFT,
    ALIGN_RIGHT,
    BOLD_ON,
    BOLD_OFF,
    PARTIAL_CUT,
    FULL_CUT,
    DRAWER_KICK_PIN0,
    DRAWER_KICK_PIN1,
    buildCut,
    buildDrawerKick,
    buildCodepageSelector,
    codepageToEscPosTable,
    normalizarComandoCorte,
    type ComandoCorte,
} from "../../src/printing/escpos/comandos";

describe("Constantes ESC/POS — bytes exactos del C#", () => {
    it("INIT_PRINTER = ESC @", () => {
        expect(Array.from(INIT_PRINTER)).toEqual([0x1b, 0x40]);
    });

    it("ALIGN_CENTER = ESC a 1", () => {
        expect(Array.from(ALIGN_CENTER)).toEqual([0x1b, 0x61, 0x01]);
    });

    it("ALIGN_LEFT = ESC a 0", () => {
        expect(Array.from(ALIGN_LEFT)).toEqual([0x1b, 0x61, 0x00]);
    });

    it("ALIGN_RIGHT = ESC a 2", () => {
        expect(Array.from(ALIGN_RIGHT)).toEqual([0x1b, 0x61, 0x02]);
    });

    it("BOLD_ON = ESC E 1", () => {
        expect(Array.from(BOLD_ON)).toEqual([0x1b, 0x45, 0x01]);
    });

    it("BOLD_OFF = ESC E 0", () => {
        expect(Array.from(BOLD_OFF)).toEqual([0x1b, 0x45, 0x00]);
    });

    it("PARTIAL_CUT = GS V B 0", () => {
        expect(Array.from(PARTIAL_CUT)).toEqual([0x1d, 0x56, 0x42, 0x00]);
    });

    it("FULL_CUT = GS V A 0", () => {
        expect(Array.from(FULL_CUT)).toEqual([0x1d, 0x56, 0x41, 0x00]);
    });

    it("DRAWER_KICK_PIN0 = ESC p 0 50 250", () => {
        expect(Array.from(DRAWER_KICK_PIN0)).toEqual([0x1b, 0x70, 0x00, 0x32, 0xfa]);
    });

    it("DRAWER_KICK_PIN1 = ESC p 1 50 250", () => {
        expect(Array.from(DRAWER_KICK_PIN1)).toEqual([0x1b, 0x70, 0x01, 0x32, 0xfa]);
    });
});

describe("buildCut", () => {
    it("None (string) → Buffer vacío", () => {
        expect(buildCut("None").length).toBe(0);
    });

    it("None (número 0) → Buffer vacío", () => {
        expect(buildCut(0).length).toBe(0);
    });

    it("PartialCut (string) → GS V B 0", () => {
        expect(Array.from(buildCut("PartialCut"))).toEqual([0x1d, 0x56, 0x42, 0x00]);
    });

    it("PartialCut (número 1) → GS V B 0", () => {
        expect(Array.from(buildCut(1))).toEqual([0x1d, 0x56, 0x42, 0x00]);
    });

    it("FullCut (string) → GS V A 0", () => {
        expect(Array.from(buildCut("FullCut"))).toEqual([0x1d, 0x56, 0x41, 0x00]);
    });

    it("FullCut (número 2) → GS V A 0", () => {
        expect(Array.from(buildCut(2))).toEqual([0x1d, 0x56, 0x41, 0x00]);
    });
});

describe("buildDrawerKick", () => {
    it("pin 0 → ESC p 0 50 250", () => {
        expect(Array.from(buildDrawerKick(0))).toEqual([0x1b, 0x70, 0x00, 0x32, 0xfa]);
    });

    it("pin 1 → ESC p 1 50 250", () => {
        expect(Array.from(buildDrawerKick(1))).toEqual([0x1b, 0x70, 0x01, 0x32, 0xfa]);
    });

    it("pin 2 (no estándar) → fallback pin 0", () => {
        expect(Array.from(buildDrawerKick(2))).toEqual([0x1b, 0x70, 0x00, 0x32, 0xfa]);
    });
});

describe("buildCodepageSelector — ESC t n", () => {
    it("CP437 → ESC t 0", () => {
        expect(Array.from(buildCodepageSelector("CP437"))).toEqual([0x1b, 0x74, 0x00]);
    });

    it("CP850 → ESC t 2", () => {
        expect(Array.from(buildCodepageSelector("CP850"))).toEqual([0x1b, 0x74, 0x02]);
    });

    it("CP858 → ESC t 19", () => {
        expect(Array.from(buildCodepageSelector("CP858"))).toEqual([0x1b, 0x74, 19]);
    });

    it("CP1252 → ESC t 16", () => {
        expect(Array.from(buildCodepageSelector("CP1252"))).toEqual([0x1b, 0x74, 16]);
    });

    it("unknown → fallback ESC t 0 (CP437)", () => {
        expect(Array.from(buildCodepageSelector("UNKNOWN"))).toEqual([0x1b, 0x74, 0x00]);
    });

    it("case insensitive", () => {
        expect(codepageToEscPosTable("cp858")).toBe(19);
        expect(codepageToEscPosTable("Cp850")).toBe(2);
    });
});

describe("normalizarComandoCorte", () => {
    it("acepta string y número equivalentes", () => {
        expect(normalizarComandoCorte("None")).toBe("None");
        expect(normalizarComandoCorte(0)).toBe("None");
        expect(normalizarComandoCorte("PartialCut")).toBe("PartialCut");
        expect(normalizarComandoCorte(1)).toBe("PartialCut");
        expect(normalizarComandoCorte("FullCut")).toBe("FullCut");
        expect(normalizarComandoCorte(2)).toBe("FullCut");
    });

    it("desconocido → None", () => {
        expect(normalizarComandoCorte(99 as ComandoCorte)).toBe("None");
    });
});
