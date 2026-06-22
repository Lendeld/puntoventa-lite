import { describe, it, expect, vi, afterEach } from "vitest";
import { esDiaAnteriorEnCR } from "@lib/utils/date.utils";

// CR es UTC-6 fijo (sin horario de verano).
describe("esDiaAnteriorEnCR", () => {
    afterEach(() => {
        vi.useRealTimers();
    });

    it("retorna true cuando la apertura fue el día CR anterior", () => {
        // Ahora: 2026-05-29 12:00 CR (18:00 UTC)
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2026-05-29T18:00:00Z"));
        // Apertura: 2026-05-28 10:00 CR
        expect(esDiaAnteriorEnCR("2026-05-28T16:00:00Z")).toBe(true);
    });

    it("retorna false cuando la apertura es el mismo día CR", () => {
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2026-05-29T18:00:00Z"));
        // Apertura: 2026-05-29 08:00 CR (14:00 UTC)
        expect(esDiaAnteriorEnCR("2026-05-29T14:00:00Z")).toBe(false);
    });

    it("trata correctamente la madrugada UTC que aún es ayer en CR", () => {
        // Ahora: 2026-05-29 00:30 UTC = 2026-05-28 18:30 CR
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2026-05-29T00:30:00Z"));
        // Apertura: 2026-05-29 02:00 UTC = 2026-05-28 20:00 CR → mismo día CR
        expect(esDiaAnteriorEnCR("2026-05-29T02:00:00Z")).toBe(false);
    });

    it("retorna false para fechas futuras", () => {
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2026-05-29T18:00:00Z"));
        expect(esDiaAnteriorEnCR("2026-05-30T16:00:00Z")).toBe(false);
    });

    it("retorna false para valores nulos o inválidos", () => {
        expect(esDiaAnteriorEnCR(null)).toBe(false);
        expect(esDiaAnteriorEnCR(undefined)).toBe(false);
        expect(esDiaAnteriorEnCR("no-es-fecha")).toBe(false);
    });
});
