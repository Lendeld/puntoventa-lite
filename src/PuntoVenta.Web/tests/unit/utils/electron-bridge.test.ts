import { describe, it, expect, afterEach } from "vitest";
import { esAppEscritorio, getBridge } from "@lib/printing/electron-bridge";

// Store the original property descriptor so we can restore it after each test.
// jsdom gives `window` a prototype chain where `pulpoImpresion` may not exist,
// so we define/delete the property directly on the global `window` object.
function setWindowBridge(value: unknown): void {
    Object.defineProperty(window, "pulpoImpresion", {
        value,
        writable: true,
        configurable: true,
    });
}

function removeWindowBridge(): void {
    // Delete the own property if it exists so subsequent `"in"` checks return false.
    if ("pulpoImpresion" in window) {
        // eslint-disable-next-line @typescript-eslint/no-dynamic-delete
        delete (window as unknown as Record<string, unknown>)["pulpoImpresion"];
    }
}

afterEach(() => {
    removeWindowBridge();
});

describe("esAppEscritorio", () => {
    it("returns false when window.pulpoImpresion is not defined", () => {
        removeWindowBridge();
        expect(esAppEscritorio()).toBe(false);
    });

    it("returns true when window.pulpoImpresion is an object", () => {
        setWindowBridge({ listarImpresoras: () => Promise.resolve([]) });
        expect(esAppEscritorio()).toBe(true);
    });

    it("returns false when window.pulpoImpresion is explicitly undefined", () => {
        setWindowBridge(undefined);
        expect(esAppEscritorio()).toBe(false);
    });
});

describe("getBridge", () => {
    it("returns null when bridge is not present", () => {
        removeWindowBridge();
        expect(getBridge()).toBeNull();
    });

    it("returns the bridge object when present", () => {
        const fakeBridge = { listarImpresoras: () => Promise.resolve([]) };
        setWindowBridge(fakeBridge);
        expect(getBridge()).toBe(fakeBridge);
    });
});
